using Anemos.Contracts.Services;
using Anemos.Helpers;
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
        _messenger.Register<CurveEditorResultMessage>(this, CurveEditorResultMessageHandler);

        ViewModel = App.GetService<CurvesViewModel>();
        Loaded += CurvesPage_Loaded;
        Unloaded += CurvesPage_Unloaded;
        InitializeComponent();
    }

    private async void OpenCurveEditorMessageHandler(object recipient, OpenCurveEditorMessage message)
    {
        if (_isDialogShown)
        {
            return;
        }

        var curveId = message.Value;
        var model = _curveService.GetCurve(curveId);
        if (model == null)
        {
            return;
        }

        var vm = App.GetService<CurveEditorViewModel>();
        vm.Id = curveId;

        SetDialogStyle();

        _isDialogShown = true;

        var result = await ViewModel.Editor.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            model.Points = _points.ToList();
        }

        _points = Enumerable.Empty<Point2>();
        _isDialogShown = false;
    }

    private void SetDialogStyle()
    {
        var dialog = ViewModel.Editor;
        dialog.XamlRoot = XamlRoot;
        dialog.Style = App.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "CurveEditorDialog_Title".GetLocalized();
        dialog.PrimaryButtonText = "Dialog_OK".GetLocalized();
        dialog.CloseButtonText = "Dialog_Cancel".GetLocalized();
        dialog.DefaultButton = ContentDialogButton.Primary;
    }

    private void CurveEditorResultMessageHandler(object recipient, CurveEditorResultMessage message)
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
