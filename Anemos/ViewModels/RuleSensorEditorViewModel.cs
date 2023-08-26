using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class RuleSensorEditorViewModel : ObservableObject
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    internal double MinTemperature => ICurveService.AbsoluteMinTemperature;
    internal double MaxTemperature => ICurveService.AbsoluteMaxTemperature;

    private string _sensorId = string.Empty;
    public string SensorId
    {
        get => _sensorId;
        set
        {
            if (SetProperty(ref _sensorId, value) && _sensorId != Source?.Id)
            {
                OnPropertyChanged(nameof(Source));
            }
        }
    }

    public SensorModelBase? Source
    {
        get => _sensorService.GetSensor(SensorId);
        set => SensorId = value?.Id ?? string.Empty;
    }

    public List<SensorModelBase> Sensors => _sensorService.Sensors;

    public double LowerValue
    {
        get; set;
    }

    public double UpperValue
    {
        get; set;
    }

    public bool IncludeLower => IndexIncludeLower > 0;

    public bool IncludeUpper => IndexIncludeUpper > 0;

    private int _indexIncludeLower;
    public int IndexIncludeLower
    {
        get => _indexIncludeLower;
        set
        {
            if (SetProperty(ref _indexIncludeLower, value))
            {
                OnPropertyChanged(nameof(IncludeLower));
            }
        }
    }

    private int _indexIncludeUpper;
    public int IndexIncludeUpper
    {
        get => _indexIncludeUpper;
        set
        {
            if (SetProperty(ref _indexIncludeUpper, value))
            {
                OnPropertyChanged(nameof(IncludeUpper));
            }
        }
    }

    public string[] signs =
    {
        "RuleSensorEditor_Signs_LT".GetLocalized(),
        "RuleSensorEditor_Signs_LEQ".GetLocalized()
    };
}
