using LibreHardwareMonitor.Hardware;

namespace Anemos.Models;

public class HWSensorModel : SensorModelBase
{
    private readonly ISensor _iSensor;

    public override string Name => _iSensor.Name.ToString();

    public override string LongName
    {
        get
        {
            if (_iSensor.Hardware.Parent == null)
            {
                return $"{Name} : {_iSensor.Hardware.Name}";
            }
            return $"{Name} : {_iSensor.Hardware.Parent.Name}";
        }
    }

    public HWSensorModel(ISensor sensor)
    {
        _iSensor = sensor;
        _id = sensor.Identifier.ToString();
        Update();
    }

    public override void Update()
    {
        Value = _iSensor.Value == null ? null : decimal.Round((decimal)_iSensor.Value, 1);
    }
}
