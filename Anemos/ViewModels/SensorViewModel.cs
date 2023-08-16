using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        => _sensorService.Sensors.Where(m => !Model.SourceIds.Contains(m.Id) && m.Id != Model.Id);

    public string[] CalcMethodNames { get; } = new[] { "Max", "Min", "Average", "Moving Average" };


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

    public SensorViewModel(CustomSensorModel model)
    {
        _messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        Model = model;
        SelectedMethodIndex = (int)Model.CalcMethod;

    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        OnPropertyChanged(nameof(SensorsNotInSources));

        var removed = message.OldValue.Except(message.NewValue).Select(m => m.Id);
        if (removed.Any())
        {
            foreach (var id in Model.SourceIds)
            {
                if (removed.Contains(id))
                {
                    Model.SourceIds.Remove(id);
                }
            }
        }
    }

    [RelayCommand]
    public void AddSource(string id)
    {
        Model.AddSource(id);
        OnPropertyChanged(nameof(SensorsNotInSources));
    }

    public void RemoveSource(string id)
    {
        Model.RemoveSource(id);
        OnPropertyChanged(nameof(SensorsNotInSources));
    }

    public void RemoveSelf()
    {
        _messenger.UnregisterAll(this);
        _sensorService.RemoveCustomSensor(Model.Id);
    }
}
