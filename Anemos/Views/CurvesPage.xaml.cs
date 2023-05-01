using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurvesPage : Page
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();

    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    private IEnumerable<Point2> _points = Enumerable.Empty<Point2>();

    private Tuple<double, double, double, double> _latchValues = new(0, 0, 0, 0);

    private bool _isDialogShown = false;

    public CurvesViewModel ViewModel
    {
        get;
    }

    public CurvesPage()
    {
        _messenger.Register<OpenCurveEditorMessage>(this, OpenCurveEditorMessageHandler);
        _messenger.Register<ChartCurveEditorResultMessage>(this, ChartCurveEditorResultMessageHandler);
        _messenger.Register<LatchCurveEditorResultMessage>(this, LatchCurveEditorResultMessageHandler);

        ViewModel = App.GetService<CurvesViewModel>();
        Loaded += CurvesPage_Loaded;
        Unloaded += CurvesPage_Unloaded;
        InitializeComponent();
    }

    private void OpenCurveEditorMessageHandler(object recipient, OpenCurveEditorMessage message)
    {
        if (_isDialogShown) { return; }

        var curveId = message.Value;
        var model = _curveService.GetCurve(curveId);
        if (model == null) { return; }

        if (model is ChartCurveModel curve)
        {
            OpenChartEditor(curve);
        }
        else if (model is LatchCurveModel latch)
        {
            OpenLatchEditor(latch);
        }
    }

    private async void OpenChartEditor(ChartCurveModel model)
    {
        var vm = App.GetService<ChartCurveEditorViewModel>();
        vm.Id = model.Id;

        SetChartEditorDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.ChartEditor.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            model.Points = _points.ToList();
        }

        _points = Enumerable.Empty<Point2>();
        _isDialogShown = false;
    }

    private async void OpenLatchEditor(LatchCurveModel model)
    {
        var vm = App.GetService<LatchCurveEditorViewModel>();
        vm.Id = model.Id;

        SetLatchEditorDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.LatchEditor.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            model.TemperatureThresholdLow = _latchValues.Item1;
            model.TemperatureThresholdHigh = _latchValues.Item2;
            model.OutputLowTemperature = _latchValues.Item3;
            model.OutputHighTemperature = _latchValues.Item4;
            model.Update();

            var view = ViewModel.Views.Single(v => v.ViewModel.Model == model);
            if (view.ViewModel is LatchCurveViewModel latch)
            {
                view.SetArrowLength(latch);
            }

            _curveService.Save();
        }

        _isDialogShown = false;
    }

    private void SetChartEditorDialogStyle()
    {
        var dialog = ViewModel.ChartEditor;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "CurveEditorDialog_Title".GetLocalized();
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void SetLatchEditorDialogStyle()
    {
        var dialog = ViewModel.LatchEditor;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "CurveEditorDialog_Title".GetLocalized();
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void ChartCurveEditorResultMessageHandler(object recipient, ChartCurveEditorResultMessage message)
    {
        _points = message.Value;
    }

    private void LatchCurveEditorResultMessageHandler(object recipient, LatchCurveEditorResultMessage message)
    {
        _latchValues = message.Value;
    }

    private void CurvesPage_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = true;
    }

    private void CurvesPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.IsVisible = false;
    }
}
