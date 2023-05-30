using System.Data.SqlClient;
using IotEmulator.Common;
using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace IoTEmulator;

public partial class EmulatorMainForm : Form
{
    private readonly BindingSource _bindingSource = new();
    private readonly List<SensorEmulator> _emulators = new();

    public EmulatorMainForm()
    {
        InitializeComponent();

        emulatorsDataGridView.DataSource = _bindingSource;
        emulatorsDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Добавление столбцов в DataGridView
        var deviceIdColumn = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(SensorEmulator.DeviceId), // это свойство должно существовать в классе SensorEmulator
            HeaderText = "Device ID",
            Name = "deviceIdColumn",
            ReadOnly = true,
            FillWeight = 25f
        };
        emulatorsDataGridView.Columns.Add(deviceIdColumn);

        // Добавление столбцов в DataGridView
        var statusColumn = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(SensorEmulator.Status), // это свойство должно существовать в классе SensorEmulator
            HeaderText = "Device status",
            Name = "statusColumn",
            ReadOnly = true,
            FillWeight = 7.5f
        };
        emulatorsDataGridView.Columns.Add(statusColumn);

        // Добавление столбцов в DataGridView
        var emulatorStatusColumn = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(SensorEmulator.EmulatorStatus), // это свойство должно существовать в классе SensorEmulator
            HeaderText = "Emulator running",
            Name = "emulatorStatusColumn",
            ReadOnly = true,
            FillWeight = 7.5f
        };
        emulatorsDataGridView.Columns.Add(emulatorStatusColumn);

        // Добавление кнопок запуска и остановки
        var startButtonColumn = new DataGridViewButtonColumn
        {
            HeaderText = "Start",
            Name = "startButtonColumn",
            Text = "Start",
            UseColumnTextForButtonValue = true,
            FillWeight = 7.5f
        };
        emulatorsDataGridView.Columns.Add(startButtonColumn);

        // Создание столбца для кнопки "Stop"
        var stopButtonColumn = new DataGridViewButtonColumn
        {
            HeaderText = "Stop",
            Name = "stopButtonColumn",
            Text = "Stop",
            UseColumnTextForButtonValue = true,
            FillWeight = 7.5f
        };
        emulatorsDataGridView.Columns.Add(stopButtonColumn);

        var sendNormalDataButtonColumn = new DataGridViewButtonColumn()
        {
            HeaderText = "Send normal data",
            Name = "sendNormalDataButtonColumn",
            Text = "Send normal data",
            UseColumnTextForButtonValue = true,
            FillWeight = 12.5f
        };
        emulatorsDataGridView.Columns.Add(sendNormalDataButtonColumn);

        var sendTooLowDataButtonColumn = new DataGridViewButtonColumn()
        {
            HeaderText = "Send too low data",
            Name = "sendToLowDataButtonColumn",
            Text = "Send too low data",
            UseColumnTextForButtonValue = true,
            FillWeight = 12.5f
        };
        emulatorsDataGridView.Columns.Add(sendTooLowDataButtonColumn);

        var sendTooHighDataButtonColumn = new DataGridViewButtonColumn()
        {
            HeaderText = "Send too high data",
            Name = "sendTooHighDataButtonColumn",
            Text = "Send too high data",
            UseColumnTextForButtonValue = true,
            FillWeight = 12.5f
        };
        emulatorsDataGridView.Columns.Add(sendTooHighDataButtonColumn);

        var emulatorValueToSendTypeColumn = new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(SensorEmulator.ValueToSendType), // это свойство должно существовать в классе SensorEmulator
            HeaderText = "Value to send type",
            Name = "valueToSendTypeColumn",
            ReadOnly = true,
            FillWeight = 7.5f
        };
        emulatorsDataGridView.Columns.Add(emulatorValueToSendTypeColumn);

        // Обработчик событий для нажатия на кнопки в DataGridView
        emulatorsDataGridView.CellClick += EmulatorsDataGridViewCellClick;

        GetSensorsFromDb();
    }

    // Обработчик событий нажатия на кнопки в DataGridView
    private async void EmulatorsDataGridViewCellClick(object sender, DataGridViewCellEventArgs e)
    {
        switch (e.ColumnIndex)
        {
            // Start button
            case 0:
                _emulators[e.RowIndex].Start();
                break;
            // Stop button
            case 1:
                await _emulators[e.RowIndex].StopAsync();
                break;
            case 2:
                _emulators[e.RowIndex].SendNormalData();
                break;
            case 3:
                _emulators[e.RowIndex].SendTooLowData();
                break;
            case 4:
                _emulators[e.RowIndex].SendTooHighData();
                break;
        }
    }

    private void addEmulatorButton_Click(object sender, EventArgs e)
    {
        var deviceId = deviceIdInputTextBox.Text;
        var deviceKey = deviceKeyInputTextBox.Text;

        if (string.IsNullOrWhiteSpace(deviceId) || string.IsNullOrWhiteSpace(deviceKey))
        {
            return;
        }

        var mailjetApiKey = ConfigurationProvider.Configuration["Mailing:ApiKey"];
        var mailJetApiSecret = ConfigurationProvider.Configuration["Mailing:ApiKey"];
        var emailFrom = ConfigurationProvider.Configuration["Mailing:From"];
        var newEmulator = new SensorEmulator(deviceId, deviceKey, mailjetApiKey, mailJetApiSecret, emailFrom);
        _emulators.Add(newEmulator);
        _bindingSource.Add(newEmulator);

        deviceIdInputTextBox.Text = "";
        deviceKeyInputTextBox.Text = "";
    }

    // method for getting information about sensors from DB
    private void GetSensorsFromDb()
    {
        var connectionString = ConfigurationProvider.Configuration.GetConnectionString("DefaultConnection");
        var mailjetApiKey = ConfigurationProvider.Configuration["Mailing:ApiKey"];
        var mailJetApiSecret = ConfigurationProvider.Configuration["Mailing:ApiSecret"];
        var emailFrom = ConfigurationProvider.Configuration["Mailing:From"];

        using var sqlConnection = new SqlConnection(connectionString);

        sqlConnection.Open();

        using var sqlCommand = new SqlCommand("SELECT Id, DeviceKey FROM ProcessPhaseParameterSensors", sqlConnection);
        using var sqlDataReader = sqlCommand.ExecuteReader();

        while (sqlDataReader.Read())
        {
            var deviceId = sqlDataReader.GetString(0);
            var deviceKey = sqlDataReader.GetString(1);
            var newEmulator = new SensorEmulator(deviceId, deviceKey, mailjetApiKey, mailJetApiSecret, emailFrom);
            _emulators.Add(newEmulator);
            _bindingSource.Add(newEmulator);
        }

        sqlConnection.Close();
    }
}