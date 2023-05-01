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

    private bool _isDialogShown = false;

    public CurvesViewModel ViewModel
    {
        get;
    }

    public CurvesPage()
    {
        _messenger.Register<OpenCurveEditorMessage>(this, OpenCurveEditorMessageHandler);
        _messenger.Register<ChartCurveEditorResultMessage>(this, ChartCurveEditorResultMessageHandler);

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

    private void ChartCurveEditorResultMessageHandler(object recipient, ChartCurveEditorResultMessage message)
    {
        _points = message.Value;
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
