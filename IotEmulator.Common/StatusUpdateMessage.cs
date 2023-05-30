namespace IotEmulator.Common;

public class StatusUpdateMessage
{
    public string NewStatus { get; set; } = null!;

    public DeviceStatus Status => Enum.Parse<DeviceStatus>(NewStatus);

    public StatusUpdateMessage()
    {

    }

    public StatusUpdateMessage(DeviceStatus status)
    {
        NewStatus = status.ToString();
    }
}