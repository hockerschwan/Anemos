using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurvesPage : Page
{
    public CurvesViewModel ViewModel
    {
        get;
    }

    public CurvesPage()
    {
        ViewModel = App.GetService<CurvesViewModel>();
        InitializeComponent();

        Loaded += CurvesPage_Loaded;
    }

    private void CurvesPage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= CurvesPage_Loaded;
        ViewModel.IsVisible = true;
    }

    public static async Task<bool> OpenCurveEditorDialog(string id)
    {
        var curveService = App.GetService<ICurveService>();
        var curve = curveService.GetCurve(id);
        if (curve == null) { return false; }

        if (curve is ChartCurveModel chart)
        {
            var dialog = new ChartCurveEditorDialog(chart.Points)
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                Title = "CurveEditor_Title".GetLocalized(),
                PrimaryButtonText = "Dialog_OK".GetLocalized(),
                IsSecondaryButtonEnabled = false,
                CloseButtonText = "Dialog_Cancel".GetLocalized(),
            };
            return await App.GetService<ShellPage>().OpenDialog(dialog);
        }
        else if (curve is LatchCurveModel latch)
        {
            var dialog = new LatchCurveEditorDialog(
                latch.TemperatureThresholdLow,
                latch.OutputLowTemperature,
                latch.TemperatureThresholdHigh,
                latch.OutputHighTemperature)
            {
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                PrimaryButtonStyle = Application.Current.Resources["AccentButtonStyle"] as Style,
                Title = "CurveEditor_Title".GetLocalized(),
                PrimaryButtonText = "Dialog_OK".GetLocalized(),
                IsSecondaryButtonEnabled = false,
                CloseButtonText = "Dialog_Cancel".GetLocalized(),
            };
            return await App.GetService<ShellPage>().OpenDialog(dialog);
        }

        return false;
    }

    public static async Task<bool> OpenDeleteDialog(string name)
    {
        var dialog = new ContentDialog
        {
            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
            PrimaryButtonStyle = Application.Current.Resources["DangerButtonStyle_"] as Style,
            Title = "Dialog_Delete_Title".GetLocalized(),
            PrimaryButtonText = "Dialog_Delete".GetLocalized(),
            IsSecondaryButtonEnabled = false,
            CloseButtonText = "Dialog_Cancel".GetLocalized(),
            Content = "Dialog_Delete_Content".GetLocalized().Replace("$", name),
        };
        return await App.GetService<ShellPage>().OpenDialog(dialog);
    }
}
