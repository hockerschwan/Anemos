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

    void Close();

    ISensor? GetSensor(string id);

    IEnumerable<ISensor> GetSensors(SensorType sensorType);

    void Start();
}
