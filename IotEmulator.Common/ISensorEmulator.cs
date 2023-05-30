namespace IotEmulator.Common;

public interface ISensorEmulator
{
    void Start();
    Task StopAsync();
}