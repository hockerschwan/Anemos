using Anemos.Contracts.Services;
using Anemos.Models;
using Anemos.Plot;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Anemos.Views;

public sealed partial class CurveView : UserControl
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public CurveViewModelBase ViewModel
    {
        get;
    }

    private Plot.Plot Plot1 => PlotControl1.Plot;
    private readonly Scatter Marker;
    private readonly double[] MarkerX = [double.NaN];
    private readonly double[] MarkerY = [double.NaN];

    private readonly Scatter? Chart;

    private readonly Scatter? LatchLow;
    private readonly Scatter? LatchHigh;
    private readonly Arrow? LatchArrowLow;
    private readonly Arrow? LatchArrowHigh;

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

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Plot1.MinX = _settingsService.Settings.CurveMinTemp;
        Plot1.MaxX = _settingsService.Settings.CurveMaxTemp;
        Plot1.MinY = 0;
        Plot1.MaxY = 100;

        if (ViewModel is ChartCurveViewModel chart)
        {
            Chart = new(chart.LineDataX, chart.LineDataY)
            {
                LineWidth = 2
            };

            Plot1.Plottables.Add(Chart);
        }
        else if (ViewModel is LatchCurveViewModel latch)
        {
            LatchLow = new(latch.LineDataLowTempX, latch.LineDataLowTempY);
            LatchHigh = new(latch.LineDataHighTempX, latch.LineDataHighTempY);
            LatchArrowLow = new(latch.ArrowLowX, latch.ArrowLowY);
            LatchArrowHigh = new(latch.ArrowHighX, latch.ArrowHighY);

            Plot1.Plottables.Add(LatchLow);
            Plot1.Plottables.Add(LatchHigh);
            Plot1.Plottables.Add(LatchArrowLow);
            Plot1.Plottables.Add(LatchArrowHigh);

            LatchLow.LineWidth = LatchHigh.LineWidth = LatchArrowLow.LineWidth = LatchArrowHigh.LineWidth = 2;
        }

        Marker = new(MarkerX, MarkerY)
        {
            Color = Colors.Orange,
            LineWidth = 0,
            MarkerRadius = 5
        };
        Plot1.Plottables.Add(Marker);

        PlotControl1.Refresh();

        ViewModel.CurveDataChanged += ViewModel_CurveDataChanged;
        ViewModel.CurveMarkerChanged += ViewModel_CurveMarkerChanged;
    }

    public void Close()
    {
        PlotControl1.Close();
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

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.CurveMinTemp))
        {
            Plot1.MinX = _settingsService.Settings.CurveMinTemp;
        }
        else if (e.PropertyName == nameof(_settingsService.Settings.CurveMaxTemp))
        {
            Plot1.MaxX = _settingsService.Settings.CurveMaxTemp;
        }
    }

    private void ViewModel_CurveDataChanged(object? sender, EventArgs e)
    {
        PlotControl1.Refresh();
    }

    private void ViewModel_CurveMarkerChanged(object? sender, EventArgs e)
    {
        MarkerX[0] = ViewModel.Model.Input ?? double.NaN;
        MarkerY[0] = ViewModel.Model.Output ?? double.NaN;

        PlotControl1.Refresh();
    }

    private void Border_ContextRequested(UIElement sender, Microsoft.UI.Xaml.Input.ContextRequestedEventArgs e)
    {
        if (Resources["ContextMenu"] is not MenuFlyout menu) { return; }
        if (e.OriginalSource is not FrameworkElement elm) { return; }

        if (e.TryGetPosition(elm, out var point))
        {
            menu.ShowAt(elm, point);
        }
        else
        {
            menu.ShowAt(elm);
        }
    }

    private async void DeleteSelfButton_Click(object sender, RoutedEventArgs e)
    {
        if (await CurvesPage.OpenDeleteDialog(ViewModel.Model.Name))
        {
            ViewModel.RemoveSelf();
        }
    }

    private void EditCurveButton_Click(object sender, RoutedEventArgs e)
    {
        OpenEditor();
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

    private void PlotControl1_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        var pt = e.GetCurrentPoint(PlotControl1);
        if (pt.Properties.IsLeftButtonPressed)
        {
            OpenEditor();
        }
    }

    private async void OpenEditor()
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
}
