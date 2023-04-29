using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class SensorsViewModel : ObservableRecipient
{
    private readonly ISensorService _sensorService;

    private List<CustomSensorModel> Models
    {
        get;
    } = new();

    private ObservableCollection<CustomSensorViewModel> ViewModels
    {
        get;
    }

    public ObservableCollection<CustomSensorView> Views
    {
        get;
    }

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public SensorsViewModel(ISensorService thermometerService)
    {
        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        _sensorService = thermometerService;

        if (_sensorService.CustomSensors.Any())
        {
            Models = _sensorService.CustomSensors.ConvertAll(s => s as CustomSensorModel)!;
        }
        ViewModels = new(Models.Select(m => new CustomSensorViewModel(m)));
        Views = new(ViewModels.Select(vm => new CustomSensorView(vm)));
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any())
        {
            foreach (var model in removed.Cast<CustomSensorModel>())
            {
                Models.Remove(model);

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
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        if (added.Any())
        {
            foreach (var model in added.Cast<CustomSensorModel>())
            {
                Models.Add(model);

                var vm = new CustomSensorViewModel(model);
                ViewModels.Add(vm);
                Views.Add(new CustomSensorView(vm));
                model.Update();
            }
        }
    }

    [RelayCommand]
    private void AddCustomSensor()
    {
        _sensorService.AddCustomSensor(new()
        {
            Name = "Sensors_NewSensorName".GetLocalized(),
            CalcMethod = CustomSensorCalcMethod.Max
        });
    }
}
