using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public class RuleSensorEditorViewModel : ObservableRecipient
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private string _sensorId = string.Empty;
    public string SensorId
    {
        get => _sensorId;
        set
        {
            if (SetProperty(ref _sensorId, value))
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

    private double _lowerValue = -273d;
    public double LowerValue
    {
        get => _lowerValue;
        set
        {
            value = double.Max(-273, double.Min(150, value));
            if (value > UpperValue) { return; }
            SetProperty(ref _lowerValue, value);
        }
    }

    private double _upperValue = 150d;
    public double UpperValue
    {
        get => _upperValue;
        set
        {
            value = double.Max(-273, double.Min(150, value));
            if (value < LowerValue) { return; }
            SetProperty(ref _upperValue, value);
        }
    }

    private bool _useLower;
    public bool UseLower
    {
        get => _useLower;
        set => SetProperty(ref _useLower, value);
    }

    private bool _useUpper;
    public bool UseUpper
    {
        get => _useUpper;
        set => SetProperty(ref _useUpper, value);
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

    public string[] signs = { "<", "≤" };

    public RuleSensorEditorViewModel()
    {
        Messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        OnPropertyChanged(nameof(Sensors));
    }

    public void Reset()
    {
        SensorId = string.Empty;
        LowerValue = -273d;
        UpperValue = 150d;
        UseLower = false;
        UseUpper = false;
        IndexIncludeLower = 0;
        IndexIncludeUpper = 0;
    }
}
