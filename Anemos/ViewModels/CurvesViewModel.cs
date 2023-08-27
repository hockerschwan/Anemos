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

    public ObservableCollection<CurveViewModelBase> ViewModels { get; } = new();
    public ObservableCollection<CurveView> Views { get; } = new();

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

    public CurvesViewModel(IMessenger messenger, ICurveService curveService)
    {
        _messenger = messenger;
        _curveService = curveService;

        _messenger.Register<CurvesChangedMessage>(this, CurvesChangedMessageHandler);

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
        var removed = message.OldValue.Except(message.NewValue);
        foreach (var model in removed)
        {
            var vm = ViewModels.FirstOrDefault(vm => vm?.Model.Id == model.Id, null);
            if (vm != null)
            {
                ViewModels.Remove(vm);
            }

            var v = Views.FirstOrDefault(v => (v?.ViewModel)?.Model.Id == model.Id, null);
            if (v != null)
            {
                Views.Remove(v);
            }
        }

        var added = message.NewValue.Except(message.OldValue);
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
            Points = new Point2d[] { new(30, 30), new(70, 70) }
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
