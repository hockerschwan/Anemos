using System.Collections.ObjectModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Anemos.ViewModels;

public partial class CurvesViewModel : ObservableRecipient
{
    private readonly ICurveService _curveService;

    private List<CurveModelBase> Models
    {
        get;
    }

    private ObservableCollection<CurveViewModelBase> ViewModels
    {
        get;
    }

    public ObservableCollection<CurveView> Views
    {
        get;
    }

    public ChartCurveEditorDialog ChartEditor
    {
        get;
    }

    public LatchCurveEditorDialog LatchEditor
    {
        get;
    }

    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public CurvesViewModel(ICurveService curveService)
    {
        _curveService = curveService;

        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<CurvesChangedMessage>(this, CurvesChangedMessageHandler);

        Models = _curveService.Curves.ToList();

        var vms = new List<CurveViewModelBase>();
        foreach (var model in Models)
        {
            if (model is ChartCurveModel chart)
            {
                vms.Add(new ChartCurveViewModel(chart));
            }
            else if (model is LatchCurveModel latch)
            {
                vms.Add(new LatchCurveViewModel(latch));
            }
        }
        ViewModels = new(vms);

        Views = new(ViewModels.Select(vm => new CurveView(vm)));
        ChartEditor = new();
        LatchEditor = new();
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any())
        {
            foreach (var model in removed.ToList())
            {
                Models.Remove(model);

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
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        if (added.Any())
        {
            foreach (var model in added)
            {
                Models.Add(model);

                if (model is ChartCurveModel curve)
                {
                    var vm = new ChartCurveViewModel(curve);
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
    }

    [RelayCommand]
    private void AddChartCurve()
    {
        _curveService.AddCurve(new()
        {
            Type = CurveType.Chart,
            Name = "Curves_NewCurveName".GetLocalized(),
            Points = new Point2[] { new(30, 30), new(70, 70) }
        });
    }

    [RelayCommand]
    private void AddLatchCurve()
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
