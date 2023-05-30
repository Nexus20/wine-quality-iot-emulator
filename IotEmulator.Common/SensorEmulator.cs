using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;
using IotEmulator.Common.Annotations;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;
using Message = Microsoft.Azure.Devices.Client.Message;

namespace IotEmulator.Common;

public class SensorEmulator : ISensorEmulator, INotifyPropertyChanged
{
    public const string IotHubUri = "wine-hub.azure-devices.net";
    public string DeviceId { get; }
    public DeviceStatus Status => _sensorInnerData.CurrentStatus;
    public bool EmulatorStatus
    {
        get => _emulatorStatus;
        set
        {
            if (_emulatorStatus == value) return;
            _emulatorStatus = value;
            OnPropertyChanged(nameof(EmulatorStatus));
        }
    }

    private readonly DeviceClient _deviceClient;
    private SensorInnerData _sensorInnerData = new();
    private CancellationTokenSource _cts;
    private Task _sendDataTask;
    private Task _receiveCommandsTask;
    private bool _emulatorStatus;
    private readonly TimeSpan _emailDelayTime = TimeSpan.FromHours(3);

    private readonly MailjetClient _mailjetClient;
    private readonly string _emailFrom;

    public ValueToSendType ValueToSendType
    {
        get => _sensorInnerData.ValueToSendType;
        set
        {
            if (_sensorInnerData.ValueToSendType == value) return;
            _sensorInnerData.ValueToSendType = value;
            OnPropertyChanged(nameof(ValueToSendType));
        }
    }

    public SensorEmulator(string deviceId, string deviceKey, string mailJetApiKey, string mailJetApiSecret, string emailFrom)
    {
        DeviceId = deviceId;
        _emailFrom = emailFrom;
        _deviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, deviceKey), TransportType.Mqtt);
        _mailjetClient = new MailjetClient(mailJetApiKey, mailJetApiSecret);
        _cts = new CancellationTokenSource();
        EmulatorStatus = false;
        GetInnerDataFromFile();
        OnPropertyChanged(nameof(Status));
    }

    public void Start()
    {
        EmulatorStatus = true;
        GetInnerDataFromFile();
        OnPropertyChanged(nameof(Status));
        _sendDataTask = SendDataAsync(_cts.Token);
        _receiveCommandsTask = ReceiveCommandsAsync(_cts.Token);
    }

    public async Task StopAsync()
    {
        _cts.Cancel();

        await Task.WhenAll(_sendDataTask, _receiveCommandsTask);

        _cts.Dispose();
        _cts = new CancellationTokenSource();

        SaveDataIntoFile();
        
        EmulatorStatus = false;
    }

    private async Task SendDataAsync(CancellationToken cancellation = default)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {

                if (_sensorInnerData.CurrentStatus == DeviceStatus.Created)
                {
                    await SendReadyMessageAsync(cancellation);
                    OnPropertyChanged(nameof(Status));
                }

                if (_sensorInnerData.CurrentStatus == DeviceStatus.Working)
                {
                    // Эмуляция отправки данных
                    var data = new SensorReadingsMessage
                    {
                        //Parameter = ParameterType,
                        Value = GetRandomReading(_sensorInnerData.ValueToSendType)
                    };

                    if (ValueToSendType != ValueToSendType.Normal)
                    {
                        if (!_sensorInnerData.LastEmailSentTime.HasValue || DateTime.UtcNow - _sensorInnerData.LastEmailSentTime >= _emailDelayTime)
                        {
                            await SendWarningEmailAsync();
                            _sensorInnerData.LastEmailSentTime = DateTime.UtcNow;
                        }
                    }

                    var dataJson = JsonConvert.SerializeObject(data);
                    var dataMessage = new Message(Encoding.UTF8.GetBytes(dataJson));
                    dataMessage.Properties.Add(nameof(MessageType), MessageType.Readings.ToString());
                    dataMessage.Properties.Add("DeviceId", DeviceId);
                    await _deviceClient.SendEventAsync(dataMessage, cancellation);
                    Console.WriteLine($"Sent data: {dataJson}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), cancellation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private double GetRandomReading(ValueToSendType valueToSendType)
    {
        var random = new Random();

        switch (valueToSendType)
        {
            case ValueToSendType.Normal:
                var range = (_sensorInnerData.UpperBound.Value - _sensorInnerData.LowerBound.Value) / 2;
                return _sensorInnerData.LowerBound.Value + range + (random.NextDouble() * (range / 100));
            case ValueToSendType.TooHigh:
                var upperBound = _sensorInnerData.UpperBound.Value;
                return upperBound + (random.NextDouble() * (upperBound / 100));
            case ValueToSendType.TooLow:
                var lowerBound = _sensorInnerData.LowerBound.Value;
                return lowerBound - (random.NextDouble() * (lowerBound / 100));
            default:
                throw new ArgumentOutOfRangeException(nameof(valueToSendType), valueToSendType, null);
        }
    }

    private Task SendReadyMessageAsync(CancellationToken cancellation = default)
    {
        return SendStatusUpdateMessage(DeviceStatus.Ready, cancellation);
    }

    private Task SendStandardsUpdatedMessageAsync(CancellationToken cancellation = default)
    {
        return SendStatusUpdateMessage(DeviceStatus.BoundariesUpdated, cancellation);
    }

    private Task SendStatusUpdateMessage(DeviceStatus status, CancellationToken cancellation = default)
    {
        var data = new StatusUpdateMessage(status);
        var dataJson = JsonConvert.SerializeObject(data);
        var dataMessage = new Message(Encoding.UTF8.GetBytes(dataJson));
        dataMessage.Properties.Add(nameof(MessageType), MessageType.StatusUpdate.ToString());
        dataMessage.Properties.Add("DeviceId", DeviceId);
        return _deviceClient.SendEventAsync(dataMessage, cancellation);
    }

    private async Task ReceiveCommandsAsync(CancellationToken cancellation = default)
    {

        while (!cancellation.IsCancellationRequested)
        {
            try
            {

                var receivedMessage = await _deviceClient.ReceiveAsync(cancellation);
                if (receivedMessage == null) continue;

                var messageString = Encoding.UTF8.GetString(receivedMessage.GetBytes());
                Console.WriteLine($"Received message: {messageString}");

                if (receivedMessage.Properties.ContainsKey(nameof(MessageType)))
                {
                    var messageType = Enum.Parse<MessageType>(receivedMessage.Properties[nameof(MessageType)]!);

                    if (messageType == MessageType.Standards)
                    {
                        var standardsUpdateMessage = JsonConvert.DeserializeObject<StandardsUpdateMessage>(messageString);

                        _sensorInnerData.LowerBound = standardsUpdateMessage.LowerBound;
                        _sensorInnerData.UpperBound = standardsUpdateMessage.UpperBound;

                        if(!string.IsNullOrWhiteSpace(standardsUpdateMessage.PhaseName))
                            _sensorInnerData.PhaseName = standardsUpdateMessage.PhaseName;

                        if (!string.IsNullOrWhiteSpace(standardsUpdateMessage.ParameterName))
                            _sensorInnerData.ParameterName = standardsUpdateMessage.ParameterName;

                        if (!string.IsNullOrWhiteSpace(standardsUpdateMessage.GrapeSortName))
                            _sensorInnerData.GrapeSortName = standardsUpdateMessage.GrapeSortName;

                        if (!string.IsNullOrWhiteSpace(standardsUpdateMessage.WineMaterialBatchId))
                            _sensorInnerData.WineMaterialBatchId = standardsUpdateMessage.WineMaterialBatchId;

                        if (!string.IsNullOrWhiteSpace(standardsUpdateMessage.WineMaterialBatchName))
                            _sensorInnerData.WineMaterialBatchName = standardsUpdateMessage.WineMaterialBatchName;


                        _sensorInnerData.CurrentStatus = DeviceStatus.BoundariesUpdated;

                        await SendStandardsUpdatedMessageAsync(cancellation);
                    }

                    if (messageType == MessageType.StatusUpdate)
                    {
                        var statusUpdateMessage = JsonConvert.DeserializeObject<StatusUpdateMessage>(messageString);
                        _sensorInnerData.CurrentStatus = statusUpdateMessage.Status;
                        await SendStatusUpdateMessage(statusUpdateMessage.Status, cancellation);
                        OnPropertyChanged(nameof(Status));
                    }
                }

                await _deviceClient.CompleteAsync(receivedMessage, cancellation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    private void GetInnerDataFromFile()
    {
        var fileName = $"{DeviceId}-sensor-data.json";

        if (File.Exists(fileName))
        {
            var json = File.ReadAllText(fileName);
            _sensorInnerData = JsonConvert.DeserializeObject<SensorInnerData>(json);
        }
    }

    private void SaveDataIntoFile()
    {
        var fileName = $"{DeviceId}-sensor-data.json";
        var json = JsonConvert.SerializeObject(_sensorInnerData);
        File.WriteAllText(fileName, json);
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void SendNormalData()
    {
        ValueToSendType = ValueToSendType.Normal;
    }

    public void SendTooHighData()
    {
        ValueToSendType = ValueToSendType.TooHigh;
    }

    public void SendTooLowData()
    {
        ValueToSendType = ValueToSendType.TooLow;
    }

    private Task SendWarningEmailAsync()
    {
        var request = new MailjetRequest
            {
                Resource = SendV31.Resource,
            }
            .Property(Send.Messages, new JArray
            {
                new JObject
                {
                    {
                        "From",
                        new JObject
                        {
                            { "Email", _emailFrom },
                            { "Name", "WineQuality" }
                        }
                    },
                    {
                        "To",
                        new JArray
                        {
                            new JObject
                            {
                                {
                                    "Email",
                                    "yevhen.chubarov@nure.ua"
                                },
                                {
                                    "Name",
                                    "Yevhen"
                                }
                            }
                        }
                    },
                    {
                        "Subject",
                        "Wine processing warning!"
                    },
                    {
                        "TextPart",
                        $"Warning. Your {_sensorInnerData.ParameterName ?? "parameter"} is {(ValueToSendType == ValueToSendType.TooHigh ? "too high" : "too low")}"
                    },
                    {
                        "HTMLPart",
                        $"<h3>Warning. Device {DeviceId} registered that your {_sensorInnerData.ParameterName ?? "parameter"} is {(ValueToSendType == ValueToSendType.TooHigh ? "too high" : "too low")}</h3>" +
                        "<div>Details:" +
                        "<ul>" +
                        $"<li>Device ID: {DeviceId}</li>" +
                        $"<li>Phase name: {_sensorInnerData.PhaseName}</li>" +
                        $"<li>Parameter name: {_sensorInnerData.ParameterName}</li>" +
                        $"<li>Grape sort name: {_sensorInnerData.GrapeSortName}</li>" +
                        $"<li>Wine material batch ID: {_sensorInnerData.WineMaterialBatchId}</li>" +
                        $"<li>Wine material batch name: {_sensorInnerData.WineMaterialBatchName}</li>" +
                        "</ul>" +
                        "</div>"
                    },
                    {
                        "CustomID",
                        DeviceId
                    }
                }
            });

        return _mailjetClient.PostAsync(request);
    }
}