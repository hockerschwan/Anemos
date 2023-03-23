using Anemos.Contracts.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LibreHardwareMonitor.Hardware;

namespace Anemos.Models;

public class HWSensorModel : ObservableObject, ISensorModel
{
    private readonly ISensor _iSensor;

    private readonly string _id;
    public string Id => _id;

    public string Name => _iSensor.Name.ToString();

    public string LongName
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

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public HWSensorModel(ISensor sensor)
    {
        _iSensor = sensor;
        _id = sensor.Identifier.ToString();
        Update();
    }

    public void Update()
    {
        Value = _iSensor.Value == null ? null : decimal.Round((decimal)_iSensor.Value, 1);
    }
}
