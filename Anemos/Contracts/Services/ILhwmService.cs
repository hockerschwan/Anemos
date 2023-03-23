using LibreHardwareMonitor.Hardware;

namespace Anemos.Contracts.Services;

public interface ILhwmService
{
    List<IHardware> Hardware
    {
        get;
    }

    List<ISensor> Sensors
    {
        get;
    }

    ISensor? GetSensor(string id);

    IEnumerable<ISensor> GetSensors(SensorType sensorType);

    Task InitAsync();

    void Close();
}
