using Anemos.Contracts.Models;
using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ISensorService
{
    List<ISensorModel> PhysicalSensors
    {
        get;
    }

    List<ISensorModel> CustomSensors
    {
        get;
    }

    List<ISensorModel> Sensors
    {
        get;
    }

    Task InitializeAsync();

    ISensorModel? GetSensor(string id);

    IEnumerable<ISensorModel> GetSensors(IEnumerable<string> idList);

    void AddCustomSensor(CustomSensorArg arg);

    void RemoveCustomSensor(string id);

    void Save();
}
