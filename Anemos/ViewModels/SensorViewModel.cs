using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class SensorViewModel : ObservableObject
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    public CustomSensorModel Model
    {
        get;
    }

    public IEnumerable<SensorModelBase> SensorsNotInSources
    {
        get
        {
            return Find(this);

            static IEnumerable<SensorModelBase> Find(SensorViewModel @this)
            {
                foreach (var s in @this._sensorService.Sensors)
                {
                    if (!@this.Model.SourceIds.Contains(s.Id) && s.Id != @this.Model.Id)
                    {
                        yield return s;
                    }
                }
            }
        }
    }

    public string[] CalcMethodNames
    {
        get;
    } =
    [
        "Sensor_CalcMethodNames_Max".GetLocalized(),
        "Sensor_CalcMethodNames_Min".GetLocalized(),
        "Sensor_CalcMethodNames_Average".GetLocalized(),
        "Sensor_CalcMethodNames_MovingAverage".GetLocalized()
    ];

    private int _selectedMethodIndex = -1;
    public int SelectedMethodIndex
    {
        get => _selectedMethodIndex;
        set
        {
            if (SetProperty(ref _selectedMethodIndex, value))
            {
                Model.CalcMethod = (CustomSensorCalcMethod)value;
                ShowSampleSize = Model.CalcMethod == CustomSensorCalcMethod.MovingAverage;
            }
        }
    }

    private bool _editingName;
    public bool EditingName
    {
        get => _editingName;
        set => SetProperty(ref _editingName, value);
    }

    private bool _showSampleSize = true;
    public bool ShowSampleSize
    {
        get => _showSampleSize;
        set => SetProperty(ref _showSampleSize, value);
    }

    private readonly MessageHandler<object, CustomSensorsChangedMessage> _customSensorsChangedMessageHandler;

    public SensorViewModel(CustomSensorModel model)
    {
        _customSensorsChangedMessageHandler = CustomSensorsChangedMessageHandler;
        _messenger.Register(this, _customSensorsChangedMessageHandler);

        Model = model;
        SelectedMethodIndex = (int)Model.CalcMethod;
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        OnPropertyChanged(nameof(SensorsNotInSources));

        var removed = message.OldValue.Except(message.NewValue).Select(m => m.Id).ToList();
        foreach (var item in removed)
        {
            Model.SourceIds.Remove(item);
        }
    }

    public void RemoveSelf()
    {
        _messenger.UnregisterAll(this);
        _sensorService.RemoveCustomSensor(Model.Id);
    }
}
