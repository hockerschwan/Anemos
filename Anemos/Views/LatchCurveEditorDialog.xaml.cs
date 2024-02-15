using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Plot;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class LatchCurveEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public LatchCurveEditorViewModel ViewModel
    {
        get;
    }

    private Plot.Plot Plot1 => PlotControl1.Plot;
    private readonly Scatter OutputLow;
    private readonly Scatter OutputHigh;
    private readonly Arrow ArrowLow;
    private readonly Arrow ArrowHigh;

    public LatchCurveEditorDialog(double thresholdLow, double outputLow, double thresholdHigh, double outputHigh)
    {
        ViewModel = new()
        {
            TemperatureThresholdLow = thresholdLow,
            TemperatureThresholdHigh = thresholdHigh,
            OutputLowTemperature = outputLow,
            OutputHighTemperature = outputHigh
        };
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;

        InitializeComponent();
        Loaded += LatchCurveEditorDialog_Loaded;
        Unloaded += LatchCurveEditorDialog_Unloaded;
        PrimaryButtonClick += LatchCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        Plot1.MinX = _settingsService.Settings.CurveMinTemp;
        Plot1.MaxX = _settingsService.Settings.CurveMaxTemp;
        Plot1.MinY = 0;
        Plot1.MaxY = 100;
        Plot1.BottomAxisLabel = "CurveEditor_Plot_X_Label".GetLocalized();
        Plot1.LeftAxisLabel = "CurveEditor_Plot_Y_Label".GetLocalized();

        OutputLow = new(ViewModel.LineDataLowTempX, ViewModel.LineDataLowTempY);
        OutputHigh = new(ViewModel.LineDataHighTempX, ViewModel.LineDataHighTempY);
        ArrowLow = new(ViewModel.ArrowLowX, ViewModel.ArrowLowY);
        ArrowHigh = new(ViewModel.ArrowHighX, ViewModel.ArrowHighY);

        Plot1.Plottables.Add(OutputLow);
        Plot1.Plottables.Add(OutputHigh);
        Plot1.Plottables.Add(ArrowLow);
        Plot1.Plottables.Add(ArrowHigh);

        OutputLow.LineWidth = OutputHigh.LineWidth = ArrowLow.LineWidth = ArrowHigh.LineWidth = 2;

        PlotControl1.Refresh();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        PlotControl1.Refresh();
    }

    private void SetNumberFormatter()
    {
        IncrementNumberRounder rounder = new()
        {
            Increment = 0.1,
            RoundingAlgorithm = RoundingAlgorithm.RoundHalfUp
        };

        DecimalFormatter formatter = new()
        {
            FractionDigits = 1,
            NumberRounder = rounder
        };

        NB_X_Low.NumberFormatter = NB_X_High.NumberFormatter
            = NB_Y_Low.NumberFormatter = NB_Y_High.NumberFormatter = formatter;
    }

    private void LatchCurveEditorDialog_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= LatchCurveEditorDialog_Loaded;
        SetDialogSize();
    }

    private void LatchCurveEditorDialog_Unloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= LatchCurveEditorDialog_Unloaded;
        PrimaryButtonClick -= LatchCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;

        Bindings.StopTracking();
    }

    private async void LatchCurveEditorDialog_PrimaryButtonClick(
        ContentDialog sender,
        ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        _messenger.Send<LatchCurveChangedMessage>(new(new(
            ViewModel.TemperatureThresholdLow,
            ViewModel.OutputLowTemperature,
            ViewModel.TemperatureThresholdHigh,
            ViewModel.OutputHighTemperature)));
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        SetDialogSize();
    }

    private void SetDialogSize()
    {
        DialogContent.Width = Math.Min(1000, App.MainWindow.Width - 200);
        DialogContent.Height = Math.Min(1000, App.MainWindow.Height - 250);
    }

    private void PreviewKeyDown_(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            e.Handled = true;

            if (sender is NumberBox nb)
            {
                var be = nb.GetBindingExpression(NumberBox.ValueProperty);
                be?.UpdateSource();
            }
        }
    }
}
