using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class SensorsViewModel : PageViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly ISensorService _sensorService;

    public ObservableCollection<SensorViewModel> ViewModels { get; } = new();
    public ObservableCollection<SensorView> Views { get; } = new();

    private bool _isVisible;
    public override bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public SensorsViewModel(IMessenger messenger, ISensorService sensorService)
    {
        _messenger = messenger;
        _sensorService = sensorService;

        _messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        foreach (var m in _sensorService.CustomSensors.ConvertAll(s => s as CustomSensorModel)!)
        {
            var vm = new SensorViewModel(m!);
            ViewModels.Add(vm);
            Views.Add(new SensorView(vm));
        }
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).OfType<CustomSensorModel>();
        foreach (var model in removed)
        {
            var vm = ViewModels.FirstOrDefault(vm => vm?.Model.Id == model.Id, null);
            if (vm != null)
            {
                ViewModels.Remove(vm);
            }

            var v = Views.FirstOrDefault(v => v?.ViewModel?.Model.Id == model.Id, null);
            if (v != null)
            {
                Views.Remove(v);
            }
        }

        var added = message.NewValue.Except(message.OldValue).OfType<CustomSensorModel>();
        foreach (var model in added)
        {
            var vm = new SensorViewModel(model);
            ViewModels.Add(vm);
            Views.Add(new SensorView(vm));
            model.Update();
        }
    }

    [RelayCommand]
    private void AddCustomSensor()
    {
        _sensorService.AddCustomSensor(new()
        {
            Name = "Sensors_NewSensorName".GetLocalized()
        });
    }
}
