using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class CurvesViewModel : PageViewModelBase
{
    private readonly IMessenger _messenger;
    private readonly ICurveService _curveService;

    public ObservableCollection<CurveViewModelBase> ViewModels { get; } = [];
    public ObservableCollection<CurveView> Views { get; } = [];

    private bool _isVisible;
    public override bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (SetProperty(ref _isVisible, value) && value)
            {
                foreach (var vm in ViewModels)
                {
                    vm.Update();
                }
            }
        }
    }

    private readonly MessageHandler<object, CurvesChangedMessage> _curvesChangedMessageHandler;

    public CurvesViewModel(IMessenger messenger, ICurveService curveService)
    {
        _messenger = messenger;
        _curveService = curveService;

        _curvesChangedMessageHandler = CurvesChangedMessageHandler;
        _messenger.Register(this, _curvesChangedMessageHandler);

        foreach (var model in _curveService.Curves)
        {
            if (model is ChartCurveModel chart)
            {
                var vm = new ChartCurveViewModel(chart);
                ViewModels.Add(vm);
                Views.Add(new CurveView(vm));
            }
            else if (model is LatchCurveModel latch)
            {
                var vm = new LatchCurveViewModel(latch);
                ViewModels.Add(vm);
                Views.Add(new CurveView(vm));
            }
        }
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
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
                    v.Close();
                    Views.Remove(v);
                    break;
                }
            }
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        foreach (var model in added)
        {
            if (model is ChartCurveModel chart)
            {
                var vm = new ChartCurveViewModel(chart);
                ViewModels.Add(vm);
                Views.Add(new CurveView(vm));
            }
            else if (model is LatchCurveModel latch)
            {
                var vm = new LatchCurveViewModel(latch);
                ViewModels.Add(vm);
                Views.Add(new CurveView(vm));
            }
            model.Update();
        }
    }

    [RelayCommand]
    private void AddChart()
    {
        _curveService.AddCurve(new()
        {
            Type = CurveType.Chart,
            Name = "Curves_NewCurveName".GetLocalized(),
            Points = [new(30, 30), new(70, 70)]
        });
    }

    [RelayCommand]
    private void AddLatch()
    {
        _curveService.AddCurve(new()
        {
            Type = CurveType.Latch,
            Name = "Curves_NewCurveName".GetLocalized(),
            OutputLowTemperature = 30.0,
            OutputHighTemperature = 70.0,
            TemperatureThresholdLow = 45.0,
            TemperatureThresholdHigh = 65.0
        });
    }
}
