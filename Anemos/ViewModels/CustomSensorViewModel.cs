using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class CustomSensorViewModel : ObservableRecipient
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    public CustomSensorModel Model
    {
        get; private set;
    }

    public IEnumerable<SensorModelBase> SensorsNotInSources
        => _sensorService.Sensors.Where(m => !Model.SourceIds.Contains(m.Id) && m.Id != Model.Id);

    public string[] CalcMethodNames
    {
        get;
    } = new[]
    {
        "CustomSensor_CalcMethod_Max".GetLocalized(),
        "CustomSensor_CalcMethod_Min".GetLocalized(),
        "CustomSensor_CalcMethod_Average".GetLocalized(),
        "CustomSensor_CalcMethod_MovingAverage".GetLocalized()
    };

    private int _selectedMethodIndex = -1;
    public int SelectedMethodIndex
    {
        get => _selectedMethodIndex;
        set
        {
            if (_selectedMethodIndex == value)
            {
                return;
            }

            SetProperty(ref _selectedMethodIndex, value);
            Model.CalcMethod = (CustomSensorCalcMethod)value;
            ShowSampleSize = Model.CalcMethod == CustomSensorCalcMethod.MovingAverage;
        }
    }

    [ObservableProperty]
    private bool _showSampleSize = true;

    public CustomSensorViewModel(CustomSensorModel model)
    {
        Messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        Model = model;
        Model.PropertyChanged += Model_PropertyChanged;
        SelectedMethodIndex = (int)Model.CalcMethod;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.SourceModels))
        {
            OnPropertyChanged(nameof(SensorsNotInSources));
        }
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        OnPropertyChanged(nameof(SensorsNotInSources));

        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any())
        {
            Model.SourceIds = Model.SourceIds.Where(id => !removed.Select(tm => tm.Id).Contains(id)).ToList();
        }
    }

    [RelayCommand]
    private void AddSource(string id)
    {
        Model.AddSource(id);
    }

    [RelayCommand]
    private void RemoveSource(string id)
    {
        Model.RemoveSource(id);
    }

    [RelayCommand]
    private void RemoveSelf()
    {
        Messenger.UnregisterAll(this);
        Model.PropertyChanged -= Model_PropertyChanged;
        _sensorService.RemoveCustomSensor(Model.Id);
    }
}
