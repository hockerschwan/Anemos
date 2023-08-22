using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Plottables;
using SkiaSharp;
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

    private Plot Plot1 => WinUIPlot1.Plot;
    private readonly Scatter LatchLow;
    private readonly Scatter LatchHigh;
    private readonly Scatter LatchLowToHighArrow;
    private readonly Scatter LatchHighToLowArrow;

    private readonly Color LineColor = Color.FromARGB((uint)System.Drawing.Color.CornflowerBlue.ToArgb());
    private readonly Color AxisColor = Color.FromARGB((uint)System.Drawing.Color.DarkGray.ToArgb());
    private readonly Color BackgroundColor = Color.FromARGB((uint)System.Drawing.Color.Black.ToArgb());
    private readonly Color GridColor = Color.FromHex("404040");

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
        Closed += LatchCurveEditorDialog_Closed;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        WinUIPlot1.Interaction.Actions = PlotActions.NonInteractive();

        Plot1.XAxis.Min = _settingsService.Settings.CurveMinTemp;
        Plot1.XAxis.Max = _settingsService.Settings.CurveMaxTemp;
        Plot1.YAxis.Min = 0;
        Plot1.YAxis.Max = 100;
        Plot1.XAxis.Label.Text = "CurveEditor_Plot_X_Label".GetLocalized();
        Plot1.YAxis.Label.Text = "CurveEditor_Plot_Y_Label".GetLocalized();
        Plot1.XAxis.Label.Font.Name = SKFontManager.Default.MatchCharacter('℃').FamilyName;
        Plot1.Style.ColorAxes(AxisColor);
        Plot1.Style.ColorGrids(GridColor);
        Plot1.DataBackground = Plot1.FigureBackground = BackgroundColor;

        LatchLow = Plot1.Add.Scatter(ViewModel.LineDataLowTempX, ViewModel.LineDataLowTempY, LineColor);
        LatchHigh = Plot1.Add.Scatter(ViewModel.LineDataHighTempX, ViewModel.LineDataHighTempY, LineColor);
        LatchLowToHighArrow = Plot1.Add.Scatter(ViewModel.LineDataLowToHighX, ViewModel.LineDataLowToHighY, LineColor);
        LatchHighToLowArrow = Plot1.Add.Scatter(ViewModel.LineDataHighToLowX, ViewModel.LineDataHighToLowY, LineColor);

        LatchLow.LineStyle.Width = LatchHigh.LineStyle.Width
            = LatchLowToHighArrow.LineStyle.Width = LatchHighToLowArrow.LineStyle.Width = 2;
        LatchLow.MarkerStyle.IsVisible = LatchHigh.MarkerStyle.IsVisible
            = LatchLowToHighArrow.MarkerStyle.IsVisible = LatchHighToLowArrow.MarkerStyle.IsVisible = false;

        WinUIPlot1.Refresh();
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        WinUIPlot1.Refresh();
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

        NB_X_Low.NumberFormatter = NB_X_High.NumberFormatter = NB_Y_Low.NumberFormatter = NB_Y_High.NumberFormatter = formatter;
    }

    private void LatchCurveEditorDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;

        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(100);
            _messenger.Send<LatchCurveChangedMessage>(new(new(
                ViewModel.TemperatureThresholdLow,
                ViewModel.OutputLowTemperature,
                ViewModel.TemperatureThresholdHigh,
                ViewModel.OutputHighTemperature)));
        });
    }

    private void LatchCurveEditorDialog_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= LatchCurveEditorDialog_Loaded;
        SetDialogSize();
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

    private void NB_X_High_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (double.IsNaN(args.NewValue)) { return; }

        if (args.NewValue <= ViewModel.TemperatureThresholdLow)
        {
            sender.Value = ViewModel.TemperatureThresholdLow + 1d;
        }
    }

    private void NB_X_Low_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (double.IsNaN(args.NewValue)) { return; }

        if (ViewModel.TemperatureThresholdHigh <= args.NewValue)
        {
            sender.Value = ViewModel.TemperatureThresholdHigh - 1d;
        }
    }
}
