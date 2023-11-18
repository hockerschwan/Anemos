using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public class SensorMonitorViewModel : MonitorViewModelBase
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    public new SensorMonitorModel Model
    {
        get;
    }

    public ObservableCollection<SensorModelBase> Sensors
    {
        get;
    }

    private SensorModelBase? _selectedSensor;
    public SensorModelBase? SelectedSensor
    {
        get => _selectedSensor;
        set
        {
            if (SetProperty(ref _selectedSensor, value))
            {
                Model.SourceId = _selectedSensor?.Id ?? string.Empty;
            }
        }
    }

    private readonly MessageHandler<object, CustomSensorsChangedMessage> _customSensorsChangedMessageHandler;

    public SensorMonitorViewModel(SensorMonitorModel model) : base(model)
    {
        _customSensorsChangedMessageHandler = CustomSensorsChangedMessageHandler;
        _messenger.Register(this, _customSensorsChangedMessageHandler);

        Model = model;
        Sensors = new(_sensorService.Sensors);
        _selectedSensor = _sensorService.GetSensor(Model.SourceId);
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var sensor in removed)
        {
            Sensors.Remove(sensor);
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var sensor in added)
        {
            Sensors.Add(sensor);
        }
    }
}
