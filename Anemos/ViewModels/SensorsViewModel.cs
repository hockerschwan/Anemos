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

    public ObservableCollection<SensorViewModel> ViewModels { get; } = [];
    public ObservableCollection<SensorView> Views { get; } = [];

    private bool _isVisible;
    public override bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private readonly MessageHandler<object, CustomSensorsChangedMessage> _customSensorsChangedMessageHandler;

    public SensorsViewModel(IMessenger messenger, ISensorService sensorService)
    {
        _messenger = messenger;
        _sensorService = sensorService;

        _customSensorsChangedMessageHandler = CustomSensorsChangedMessageHandler;
        _messenger.Register(this, _customSensorsChangedMessageHandler);

        foreach (var m in _sensorService.CustomSensors.ConvertAll(s => s as CustomSensorModel)!)
        {
            var vm = new SensorViewModel(m!);
            ViewModels.Add(vm);
            Views.Add(new SensorView(vm));
        }
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).OfType<CustomSensorModel>().ToList();
        foreach (var model in removed)
        {
            foreach (var vm in ViewModels)
            {
                if (vm.Model.Id == model.Id)
                {
                    ViewModels.Remove(vm);
                    break;
                }
            }

            foreach (var v in Views)
            {
                if (v.ViewModel.Model.Id == model.Id)
                {
                    Views.Remove(v);
                    break;
                }
            }
        }

        var added = message.NewValue.Except(message.OldValue).OfType<CustomSensorModel>().ToList();
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
