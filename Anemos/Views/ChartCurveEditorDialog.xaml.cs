using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Plot;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Globalization.NumberFormatting;

namespace Anemos.Views;

public sealed partial class ChartCurveEditorDialog : ContentDialog
{
    private readonly IMessenger _messenger = App.GetService<IMessenger>();
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public ChartCurveEditorViewModel ViewModel
    {
        get;
    }

    private Plot.Plot Plot1 => PlotControl1.Plot;
    private readonly Scatter Chart;
    private readonly Scatter Marker;
    private readonly double[] MarkerX = [double.NaN];
    private readonly double[] MarkerY = [double.NaN];

    private readonly float _markerSize = 3.75f;

    private bool _isDragged;

    public ChartCurveEditorDialog(IEnumerable<Point2d> points)
    {
        ViewModel = new();
        ViewModel.SetPoints(ref points);

        InitializeComponent();
        Loaded += ChartCurveEditorDialog_Loaded;
        Unloaded += ChartCurveEditorDialog_Unloaded;
        PrimaryButtonClick += ChartCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        PlotControl1.PointerMoved += WinUIPlot1_PointerMoved;
        PlotControl1.PointerPressed += WinUIPlot1_PointerPressed;
        PlotControl1.PointerReleased += WinUIPlot1_PointerReleased;

        Plot1.MinX = _settingsService.Settings.CurveMinTemp;
        Plot1.MaxX = _settingsService.Settings.CurveMaxTemp;
        Plot1.MinY = 0;
        Plot1.MaxY = 100;
        Plot1.BottomAxisLabel = "CurveEditor_Plot_X_Label".GetLocalized();
        Plot1.LeftAxisLabel = "CurveEditor_Plot_Y_Label".GetLocalized();

        Chart = new(ViewModel.LineDataX, ViewModel.LineDataY)
        {
            LineWidth = 2,
            MarkerRadius = _markerSize
        };
        Plot1.Plottables.Add(Chart);

        Marker = new(MarkerX, MarkerY)
        {
            Color = Colors.Orange,
            LineWidth = 0,
            MarkerRadius = _markerSize * 2
        };
        Plot1.Plottables.Add(Marker);

        PlotControl1.Refresh();
    }

    private int FindIndex(double value)
    {
        for (var i = 0; i < ViewModel.LineDataX.Count; ++i)
        {
            if (ViewModel.LineDataX[i] > value) { return i; }
        }
        return -1;
    }

    private double FindNextX(double value)
    {
        foreach (var x in ViewModel.LineDataX)
        {
            if (x > value) { return x; };
        }
        return double.PositiveInfinity;
    }

    private double FindPreviousX(double value)
    {
        for (var i = ViewModel.LineDataX.Count - 1; i >= 0; --i)
        {
            if (ViewModel.LineDataX[i] < value) { return ViewModel.LineDataX[i]; };
        }
        return double.NegativeInfinity;
    }

    private Point2d GetNearestPoint(double px, double py)
    {
        var points = Chart.GetScatterPoints();
        if (points.Length == 0) { return Point2d.NaN; }

        var mc_x = Plot1.GetPointX((float)px);

        var i = 0;
        for (; i < points.Length; ++i)
        {
            if (mc_x <= points[i].X) { break; }
        }

        Span<Point2d> near;
        if (i == 0)
        {
            near = points.AsSpan(0, 1);
        }
        else if (i == points.Length)
        {
            near = points.AsSpan(points.Length - 1, 1);
        }
        else
        {
            near = points.AsSpan(i - 1, 2);
        }

        var nearest = Point2d.NaN;
        var distMin = double.PositiveInfinity;
        foreach (var point in near)
        {
            var x = Plot1.GetPixelX(point.X);
            var y = Plot1.GetPixelY(point.Y);
            var dist = Math.Pow(x - px, 2) + Math.Pow(y - py, 2);
            if (dist < distMin)
            {
                distMin = dist;
                nearest = point;
            }
        }

        return distMin > double.Pow(_markerSize, 2) ? Point2d.NaN : nearest;
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

        NB_X.NumberFormatter = NB_Y.NumberFormatter = formatter;
    }

    private void UpdateInfinity(bool force = false)
    {
        if (!force && ViewModel.SelectedIndex != 1 && ViewModel.SelectedIndex != ViewModel.LineDataX.Count - 2) { return; }

        if (ViewModel.LineDataY.Count > 2)
        {
            ViewModel.LineDataY[0] = ViewModel.LineDataY[1];
            ViewModel.LineDataY[^1] = ViewModel.LineDataY[^2];
        }
        else
        {
            ViewModel.LineDataY[0] = 0;
            ViewModel.LineDataY[^1] = 0;
        }
    }

    private void WinUIPlot1_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var mp = e.GetCurrentPoint(PlotControl1).Position;
        if (_isDragged) // drag
        {
            var prev = FindPreviousX(ViewModel.SelectedX);
            var next = FindNextX(ViewModel.SelectedX);

            var low = double.Max(Plot1.MinX, prev + 0.1);
            var high = double.Min(Plot1.MaxX, next - 0.1);

            var mc_x = Plot1.GetPointX((float)mp.X);
            var mc_y = Plot1.GetPointY((float)mp.Y);

            mc_x = double.Round(double.Clamp(mc_x, low, high), 1);
            mc_y = double.Round(double.Clamp(mc_y, 0, 100), 1);

            ViewModel.LineDataX[ViewModel.SelectedIndex] = ViewModel.SelectedX = MarkerX[0] = mc_x;
            ViewModel.LineDataY[ViewModel.SelectedIndex] = ViewModel.SelectedY = MarkerY[0] = mc_y;

            UpdateInfinity();
        }
        else // select
        {
            var nearest = GetNearestPoint(mp.X, mp.Y);
            if (nearest == Point2d.NaN) { return; }
            if (nearest.X == ViewModel.SelectedX && nearest.Y == ViewModel.SelectedY) { return; }

            ViewModel.SelectedIndex = ViewModel.LineDataX.IndexOf(nearest.X);
            ViewModel.SelectedX = MarkerX[0] = nearest.X;
            ViewModel.SelectedY = MarkerY[0] = nearest.Y;

            NB_X.IsEnabled = NB_Y.IsEnabled = true;
        }
    }

    private void WinUIPlot1_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pp = e.GetCurrentPoint(this).Properties;
        var mp = e.GetCurrentPoint(PlotControl1).Position;
        if (pp.IsLeftButtonPressed) // add
        {
            _isDragged = true;
            NB_X.IsEnabled = NB_Y.IsEnabled = false;

            var nearest = GetNearestPoint(mp.X, mp.Y);
            if (nearest != Point2d.NaN) { return; }

            var mc_x = double.Round(Plot1.GetPointX((float)mp.X), 1);
            var mc_y = double.Round(Plot1.GetPointY((float)mp.Y), 1);

            var index = FindIndex(mc_x);
            ViewModel.LineDataX.Insert(index, mc_x);
            ViewModel.LineDataY.Insert(index, mc_y);

            ViewModel.SelectedIndex = index;
            ViewModel.SelectedX = MarkerX[0] = mc_x;
            ViewModel.SelectedY = MarkerY[0] = mc_y;

            UpdateInfinity();
        }
        else if (pp.IsRightButtonPressed) // delete
        {
            var px_selectedX = Plot1.GetPixelX(MarkerX[0]);
            var px_selectedY = Plot1.GetPixelY(MarkerY[0]);
            var d = double.Sqrt(double.Pow(px_selectedX - mp.X, 2) + double.Pow(px_selectedY - mp.Y, 2));
            if (d > _markerSize) { return; }
            if (ViewModel.SelectedIndex < 0 || ViewModel.SelectedIndex >= ViewModel.LineDataX.Count) { return; }

            ViewModel.LineDataX.RemoveAt(ViewModel.SelectedIndex);
            ViewModel.LineDataY.RemoveAt(ViewModel.SelectedIndex);

            MarkerX[0] = MarkerY[0] = double.NaN;

            ViewModel.SelectedX = ViewModel.SelectedY = double.NaN;
            ViewModel.SelectedIndex = -1;
            UpdateInfinity(true);

            NB_X.IsEnabled = NB_Y.IsEnabled = false;
        }
    }

    private void WinUIPlot1_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragged = false;
        NB_X.IsEnabled = NB_Y.IsEnabled = ViewModel.SelectedIndex > -1;
    }

    private void ChartCurveEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= ChartCurveEditorDialog_Loaded;
        SetDialogSize();
        NB_X.IsEnabled = NB_Y.IsEnabled = false;
    }

    private void ChartCurveEditorDialog_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Unloaded -= ChartCurveEditorDialog_Unloaded;
        PrimaryButtonClick -= ChartCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;
        PlotControl1.PointerMoved -= WinUIPlot1_PointerMoved;
        PlotControl1.PointerPressed -= WinUIPlot1_PointerPressed;
        PlotControl1.PointerReleased -= WinUIPlot1_PointerReleased;

        Bindings.StopTracking();
    }

    private async void ChartCurveEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        var points = Chart.GetScatterPoints().Skip(1).SkipLast(1).ToList();
        _messenger.Send<ChartCurveChangedMessage>(new(points));
    }

    private void MainWindow_SizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
    {
        SetDialogSize();
    }

    private void SetDialogSize()
    {
        DialogContent.Width = Math.Min(1000, App.MainWindow.Width - 200);
        DialogContent.Height = Math.Min(1000, App.MainWindow.Height - 250);
    }

    private void NB_X_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        MarkerX[0] = sender.Value = args.NewValue;

        PlotControl1.Refresh();
    }

    private void NB_Y_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        MarkerY[0] = sender.Value = args.NewValue;
        UpdateInfinity();

        PlotControl1.Refresh();
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
