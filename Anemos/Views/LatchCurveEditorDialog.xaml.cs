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
    private readonly Scatter OutputLow;
    private readonly Scatter OutputHigh;
    private readonly ArrowCoordinated ArrowLow;
    private readonly ArrowCoordinated ArrowHigh;

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
        Unloaded += LatchCurveEditorDialog_Unloaded;
        PrimaryButtonClick += LatchCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.PositionChanged += MainWindow_PositionChanged;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        WinUIPlot1.Interaction.Disable();

        Plot1.Axes.Bottom.Min = _settingsService.Settings.CurveMinTemp;
        Plot1.Axes.Bottom.Max = _settingsService.Settings.CurveMaxTemp;
        Plot1.Axes.Left.Min = 0;
        Plot1.Axes.Left.Max = 100;
        Plot1.Axes.Bottom.Label.Text = "CurveEditor_Plot_X_Label".GetLocalized();
        Plot1.Axes.Left.Label.Text = "CurveEditor_Plot_Y_Label".GetLocalized();
        Plot1.Axes.Bottom.Label.FontName = Plot1.Axes.Left.Label.FontName = SKFontManager.Default.MatchCharacter('℃').FamilyName;
        Plot1.Style.ColorAxes(AxisColor);
        Plot1.Style.ColorGrids(GridColor);
        Plot1.DataBackground = Plot1.FigureBackground = BackgroundColor;
        Plot1.ScaleFactor = (float)App.MainWindow.DisplayScale;

        OutputLow = Plot1.Add.Scatter(ViewModel.LineDataLowTempX, ViewModel.LineDataLowTempY, LineColor);
        OutputHigh = Plot1.Add.Scatter(ViewModel.LineDataHighTempX, ViewModel.LineDataHighTempY, LineColor);
        ArrowLow = new(ViewModel.ArrowLowCoordinates);
        ArrowHigh = new(ViewModel.ArrowHighCoordinates);
        Plot1.Add.Plottable(ArrowLow);
        Plot1.Add.Plottable(ArrowHigh);

        OutputLow.LineStyle.Width = OutputHigh.LineStyle.Width = ArrowLow.LineStyle.Width = ArrowHigh.LineStyle.Width = 2;
        OutputLow.MarkerStyle.IsVisible = OutputHigh.MarkerStyle.IsVisible = false;
        ArrowLow.LineStyle.Color = ArrowHigh.LineStyle.Color = LineColor;

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

    private void LatchCurveEditorDialog_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= LatchCurveEditorDialog_Loaded;
        SetDialogSize();
    }

    private void LatchCurveEditorDialog_Unloaded(object sender, RoutedEventArgs e)
    {
        Unloaded -= LatchCurveEditorDialog_Unloaded;
        PrimaryButtonClick -= LatchCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.PositionChanged -= MainWindow_PositionChanged;
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;

        Bindings.StopTracking();
    }

    private async void LatchCurveEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
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

    private void MainWindow_PositionChanged(object? sender, Windows.Graphics.PointInt32 e)
    {
        var scale = (float)App.MainWindow.DisplayScale;
        if (Plot1.ScaleFactor != scale)
        {
            Plot1.ScaleFactor = scale;
            WinUIPlot1.Refresh();
        }
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
