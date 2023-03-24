using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ISensorService
{
    List<SensorModelBase> PhysicalSensors
    {
        get;
    }

    List<SensorModelBase> CustomSensors
    {
        get;
    }

    List<SensorModelBase> Sensors
    {
        get;
    }

    Task InitializeAsync();

    SensorModelBase? GetSensor(string id);

    IEnumerable<SensorModelBase> GetSensors(IEnumerable<string> idList);

    void AddCustomSensor(CustomSensorArg arg);

    void RemoveCustomSensor(string id);

    void Save();
}
