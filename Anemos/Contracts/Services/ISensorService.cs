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

    void AddCustomSensor(CustomSensorArg arg);

    SensorModelBase? GetSensor(string id);

    IEnumerable<SensorModelBase> GetSensors(IEnumerable<string> idList);

    Task LoadAsync();

    void RemoveCustomSensor(string id);

    void Save();
}
