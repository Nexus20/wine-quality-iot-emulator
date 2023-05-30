namespace IotEmulator.Common;

public enum DeviceStatus
{
    Created = 0,
    Ready = 1000,
    BoundariesUpdated = 2000,
    Working = 3000,
    Stopped = 4000
}