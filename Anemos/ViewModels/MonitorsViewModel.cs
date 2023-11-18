using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class MonitorsViewModel : PageViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly INotifyIconMonitorService _monitorService;

    public ObservableCollection<MonitorViewModelBase> ViewModels { get; } = [];
    public ObservableCollection<MonitorViewBase> Views { get; } = [];

    private bool _isVisible;
    public override bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    private readonly MessageHandler<object, MonitorsChangedMessage> _monitorsChangedMessageHandler;

    public MonitorsViewModel(IMessenger messenger, INotifyIconMonitorService monitorService)
    {
        _messenger = messenger;
        _monitorService = monitorService;

        _monitorsChangedMessageHandler = MonitorsChangedMessageHandler;
        _messenger.Register(this, _monitorsChangedMessageHandler);

        foreach (var model in _monitorService.Monitors)
        {
            switch (model.SourceType)
            {
                case MonitorSourceType.Fan:
                    {
                        var vm = new FanMonitorViewModel((FanMonitorModel)model);
                        ViewModels.Add(vm);
                        Views.Add(new FanMonitorView(vm));
                        break;
                    }
                case MonitorSourceType.Curve:
                    {
                        var vm = new CurveMonitorViewModel((CurveMonitorModel)model);
                        ViewModels.Add(vm);
                        Views.Add(new CurveMonitorView(vm));
                        break;
                    }
                case MonitorSourceType.Sensor:
                    {
                        var vm = new SensorMonitorViewModel((SensorMonitorModel)model);
                        ViewModels.Add(vm);
                        Views.Add(new SensorMonitorView(vm));
                        break;
                    }
            }
        }
    }

    private void MonitorsChangedMessageHandler(object recipient, MonitorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
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

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var model in added)
        {
            if (model is FanMonitorModel fan)
            {
                var vm = new FanMonitorViewModel(fan);
                ViewModels.Add(vm);
                Views.Add(new FanMonitorView(vm));
            }
            else if (model is CurveMonitorModel curve)
            {
                var vm = new CurveMonitorViewModel(curve);
                ViewModels.Add(vm);
                Views.Add(new CurveMonitorView(vm));
            }
            else if (model is SensorMonitorModel sensor)
            {
                var vm = new SensorMonitorViewModel(sensor);
                ViewModels.Add(vm);
                Views.Add(new SensorMonitorView(vm));
            }
            model.Update();
        }
    }

    [RelayCommand]
    private void AddMonitor(string typeStr)
    {
        if (!Enum.TryParse(typeof(MonitorSourceType), typeStr, out var type)) { return; }

        var id = Guid.NewGuid().ToString();
        _monitorService.AddMonitor(new()
        {
            SourceType = (MonitorSourceType)type,
            Id = id,
            Colors = new Tuple<double, string>[] { new(double.NegativeInfinity, "FFFFFF") }
        });
        _monitorService.GetMonitor(id)?.NotifyIcon.SetVisibility(true);
    }
}
