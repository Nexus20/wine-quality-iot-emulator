namespace IotEmulator.Common;

public class SensorInnerData
{
    public double? UpperBound { get; set; }
    public double? LowerBound { get; set; }
    public string? PhaseName { get; set; }
    public string? ParameterName { get; set; }
    public string? GrapeSortName { get; set; }
    public string? WineMaterialBatchId { get; set; }
    public string? WineMaterialBatchName { get; set; }
    public DateTime? LastEmailSentTime;

    public DeviceStatus CurrentStatus = DeviceStatus.Created;
    public ValueToSendType ValueToSendType = ValueToSendType.Normal;
}