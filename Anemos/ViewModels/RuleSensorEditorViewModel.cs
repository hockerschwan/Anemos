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
        LowerValue = double.NaN;
        UpperValue = double.NaN;
        IndexIncludeLower = 0;
        IndexIncludeUpper = 0;
    }
}
