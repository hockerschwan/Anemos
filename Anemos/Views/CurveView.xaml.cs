using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Plottables;

namespace Anemos.Views;

public sealed partial class CurveView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public CurveViewModelBase ViewModel
    {
        get;
    }

    private Plot Plot1 => WinUIPlot1.Plot;
    private readonly Scatter Marker;
    private readonly Coordinates[] MarkerCoordinates = [Coordinates.NaN];

    private readonly Color LineColor = Color.FromARGB((uint)System.Drawing.Color.CornflowerBlue.ToArgb());
    private readonly Color MarkerColor = Color.FromARGB((uint)System.Drawing.Color.Orange.ToArgb());
    private readonly Color AxisColor = Color.FromARGB((uint)System.Drawing.Color.DarkGray.ToArgb());
    private readonly Color BackgroundColor = Color.FromARGB((uint)System.Drawing.Color.Black.ToArgb());
    private readonly Color GridColor = Color.FromHex("404040");

    private readonly Scatter? Chart;

    private readonly Scatter? LatchLow;
    private readonly Scatter? LatchHigh;
    private readonly ArrowCoordinated? LatchArrowLow;
    private readonly ArrowCoordinated? LatchArrowHigh;

    private bool _chartEditorOpened;
    private bool _latchEditorOpened;

    private readonly MessageHandler<object, ChartCurveChangedMessage> _chartCurveChangedMessageHandler;
    private readonly MessageHandler<object, LatchCurveChangedMessage> _latchCurveChangedMessageHandler;

    public CurveView(CurveViewModelBase viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        _chartCurveChangedMessageHandler = ChartCurveChangedMessageHandler;
        _latchCurveChangedMessageHandler = LatchCurveChangedMessageHandler;
        _messenger.Register(this, _chartCurveChangedMessageHandler);
        _messenger.Register(this, _latchCurveChangedMessageHandler);

        App.MainWindow.PositionChanged += MainWindow_PositionChanged;
        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        WinUIPlot1.Interaction.Actions = ScottPlot.Control.PlotActions.NonInteractive();

        Plot1.XAxis.Min = _settingsService.Settings.CurveMinTemp;
        Plot1.XAxis.Max = _settingsService.Settings.CurveMaxTemp;
        Plot1.YAxis.Min = 0;
        Plot1.YAxis.Max = 100;
        Plot1.Style.ColorAxes(AxisColor);
        Plot1.Style.ColorGrids(GridColor);
        Plot1.DataBackground = Plot1.FigureBackground = BackgroundColor;
        Plot1.ScaleFactor = (float)App.MainWindow.DisplayScale;

        if (ViewModel is ChartCurveViewModel chart)
        {
            Chart = Plot1.Add.Scatter(chart.LineDataX, chart.LineDataY, LineColor);
            Chart.LineStyle.Width = 2;
            Chart.MarkerStyle.IsVisible = false;
        }
        else if (ViewModel is LatchCurveViewModel latch)
        {
            LatchLow = Plot1.Add.Scatter(latch.LineDataLowTempX, latch.LineDataLowTempY, LineColor);
            LatchHigh = Plot1.Add.Scatter(latch.LineDataHighTempX, latch.LineDataHighTempY, LineColor);
            LatchArrowLow = new(latch.ArrowLowCoordinates);
            LatchArrowHigh = new(latch.ArrowHighCoordinates);
            Plot1.Add.Plottable(LatchArrowLow);
            Plot1.Add.Plottable(LatchArrowHigh);

            LatchLow.LineStyle.Width = LatchHigh.LineStyle.Width
                = LatchArrowLow.LineStyle.Width = LatchArrowHigh.LineStyle.Width = 2;
            LatchLow.MarkerStyle.IsVisible = LatchHigh.MarkerStyle.IsVisible = false;
            LatchArrowLow.LineStyle.Color = LatchArrowHigh.LineStyle.Color = LineColor;
        }

        Marker = Plot1.Add.Scatter(new ScatterSourceCoordinates(MarkerCoordinates), MarkerColor);
        Marker.MarkerStyle.Size = 10;

        WinUIPlot1.Refresh();

        ViewModel.CurveDataChanged += ViewModel_CurveDataChanged;
        ViewModel.CurveMarkerChanged += ViewModel_CurveMarkerChanged;
    }

    private void ChartCurveChangedMessageHandler(object recipient, ChartCurveChangedMessage message)
    {
        if (!_chartEditorOpened || ViewModel.Model is not ChartCurveModel chart) { return; }

        _chartEditorOpened = false;
        chart.Points = message.Value;
    }

    private void LatchCurveChangedMessageHandler(object recipient, LatchCurveChangedMessage message)
    {
        if (!_latchEditorOpened || ViewModel.Model is not LatchCurveModel latch) { return; }

        _latchEditorOpened = false;
        latch.TemperatureThresholdLow = message.Value.Item1;
        latch.OutputLowTemperature = message.Value.Item2;
        latch.TemperatureThresholdHigh = message.Value.Item3;
        latch.OutputHighTemperature = message.Value.Item4;
        latch.Update();
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

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.CurveMinTemp))
        {
            Plot1.XAxis.Min = _settingsService.Settings.CurveMinTemp;
        }
        else if (e.PropertyName == nameof(_settingsService.Settings.CurveMaxTemp))
        {
            Plot1.XAxis.Max = _settingsService.Settings.CurveMaxTemp;
        }
    }

    private void ViewModel_CurveDataChanged(object? sender, EventArgs e)
    {
        WinUIPlot1.Refresh();
    }

    private void ViewModel_CurveMarkerChanged(object? sender, EventArgs e)
    {
        MarkerCoordinates[0].X = ViewModel.Model.Input ?? double.NaN;
        MarkerCoordinates[0].Y = ViewModel.Model.Output ?? double.NaN;

        WinUIPlot1.Refresh();
    }

    private async void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
    {
        if (await CurvesPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private async void EditCurveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.Model is ChartCurveModel chart)
        {
            _chartEditorOpened = await CurvesPage.OpenCurveEditorDialog(chart.Id);
        }
        else if (ViewModel.Model is LatchCurveModel latch)
        {
            _latchEditorOpened = await CurvesPage.OpenCurveEditorDialog(latch.Id);
        }
    }

    private void EditNameTextBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not TextBox tb) { return; }

        tb.Focus(FocusState.Programmatic);
        tb.SelectionStart = tb.Text.Length;
    }

    private void EditNameTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Escape:
                (sender as TextBox)!.Text = ViewModel.Model.Name;
                ViewModel.EditingName = false;
                break;
            case Windows.System.VirtualKey.Enter:
                ViewModel.EditingName = false;
                break;
        }
    }

    private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        PlotOverlay.Visibility = Visibility.Visible;
    }

    private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        PlotOverlay.Visibility = Visibility.Collapsed;
    }
}
