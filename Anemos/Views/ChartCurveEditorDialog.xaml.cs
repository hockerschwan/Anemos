using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ScottPlot;
using ScottPlot.Plottables;
using SkiaSharp;
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

    private Plot Plot1 => WinUIPlot1.Plot;
    private readonly Scatter Chart;
    private readonly Scatter Marker;
    private readonly Coordinates[] MarkerCoordinates = [Coordinates.NaN];

    private readonly Color LineColor = Color.FromARGB((uint)System.Drawing.Color.CornflowerBlue.ToArgb());
    private readonly Color MarkerColor = Color.FromARGB((uint)System.Drawing.Color.Orange.ToArgb());
    private readonly Color AxisColor = Color.FromARGB((uint)System.Drawing.Color.DarkGray.ToArgb());
    private readonly Color BackgroundColor = Color.FromARGB((uint)System.Drawing.Color.Black.ToArgb());
    private readonly Color GridColor = Color.FromHex("404040");

    private readonly float _markerSize = 7.5f;

    private bool _isDragged;

    public ChartCurveEditorDialog(IEnumerable<Point2d> points)
    {
        ViewModel = new();
        ViewModel.SetPoints(ref points);

        InitializeComponent();
        Loaded += ChartCurveEditorDialog_Loaded;
        Unloaded += ChartCurveEditorDialog_Unloaded;
        PrimaryButtonClick += ChartCurveEditorDialog_PrimaryButtonClick;
        App.MainWindow.PositionChanged += MainWindow_PositionChanged;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        WinUIPlot1.Interaction.Disable();
        WinUIPlot1.PointerMoved += WinUIPlot1_PointerMoved;
        WinUIPlot1.PointerPressed += WinUIPlot1_PointerPressed;
        WinUIPlot1.PointerReleased += WinUIPlot1_PointerReleased;

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

        Chart = Plot1.Add.Scatter(ViewModel.LineDataX, ViewModel.LineDataY, LineColor);
        Chart.LineStyle.Width = 2;
        Chart.MarkerStyle.Size = _markerSize;

        Marker = Plot1.Add.Scatter(MarkerCoordinates);
        Marker.MarkerStyle.Size = _markerSize * 2;
        Marker.MarkerStyle.Fill.Color = MarkerColor;

        WinUIPlot1.Refresh();
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

    private Coordinates GetNearestCoordinatesInRange(double x, double y)
    {
        var px = new Pixel(x, y);
        var mc = Plot1.GetCoordinates(new(x * Plot1.ScaleFactor, y * Plot1.ScaleFactor));

        var points = Chart.Data.GetScatterPoints();
        if (!points.Any()) { return Coordinates.NaN; }

        var i = 0;
        for (; i < points.Count; ++i)
        {
            if (mc.X <= points[i].X) { break; }
        }

        IEnumerable<Coordinates> near;
        if (i == 0)
        {
            near = points.Take(1);
        }
        else if (i == points.Count)
        {
            near = points.TakeLast(1);
        }
        else
        {
            near = points.Skip(i - 1).Take(2);
        }

        var nearest = near.First();
        var distMin = double.PositiveInfinity;
        foreach (var point in near.ToList())
        {
            var dist = point.Distance(mc);
            if (dist < distMin)
            {
                distMin = dist;
                nearest = point;
            }
        }

        var nearest_scaled = new Coordinates(nearest.X / Plot1.ScaleFactor, nearest.Y / Plot1.ScaleFactor);
        var px_d = Plot1.GetPixel(nearest_scaled) - px;

        var d = double.Sqrt(double.Pow(px_d.X, 2) + double.Pow(px_d.Y, 2));
        return d > _markerSize ? Coordinates.NaN : nearest;
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
        if (_isDragged) // drag
        {
            var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;

            var prev = FindPreviousX(ViewModel.SelectedX);
            var next = FindNextX(ViewModel.SelectedX);

            var axisLimits = Plot1.Axes.GetLimits();
            var low = double.Max(axisLimits.XRange.Min, prev + 0.1);
            var high = double.Min(axisLimits.XRange.Max, next - 0.1);

            var mc = Plot1.GetCoordinates(new Pixel(mp.X * Plot1.ScaleFactor, mp.Y * Plot1.ScaleFactor));

            var mc_x = double.Round(double.Clamp(mc.X, low, high), 1);
            var mc_y = double.Round(double.Clamp(mc.Y, 0, 100), 1);

            ViewModel.LineDataX[ViewModel.SelectedIndex] = ViewModel.SelectedX = MarkerCoordinates[0].X = mc_x;
            ViewModel.LineDataY[ViewModel.SelectedIndex] = ViewModel.SelectedY = MarkerCoordinates[0].Y = mc_y;

            UpdateInfinity();
        }
        else // select
        {
            var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;
            var nearest = GetNearestCoordinatesInRange(mp.X, mp.Y);
            if (nearest == Coordinates.NaN) { return; }
            if (nearest.X == ViewModel.SelectedX && nearest.Y == ViewModel.SelectedY) { return; }

            ViewModel.SelectedIndex = ViewModel.LineDataX.IndexOf(nearest.X);
            ViewModel.SelectedX = MarkerCoordinates[0].X = nearest.X;
            ViewModel.SelectedY = MarkerCoordinates[0].Y = nearest.Y;
            Marker.IsVisible = true;

            NB_X.IsEnabled = NB_Y.IsEnabled = true;
        }
    }

    private void WinUIPlot1_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pp = e.GetCurrentPoint(this).Properties;
        var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;
        if (pp.IsLeftButtonPressed) // add
        {
            _isDragged = true;
            NB_X.IsEnabled = NB_Y.IsEnabled = false;

            var nearest = GetNearestCoordinatesInRange(mp.X, mp.Y);
            if (nearest != Coordinates.NaN) { return; }

            var px = new Pixel(mp.X * Plot1.ScaleFactor, mp.Y * Plot1.ScaleFactor);
            var mc = Plot1.GetCoordinates(px);

            var mc_x = double.Round(mc.X, 1);
            var mc_y = double.Round(mc.Y, 1);

            var index = FindIndex(mc_x);
            ViewModel.LineDataX.Insert(index, mc_x);
            ViewModel.LineDataY.Insert(index, mc_y);

            ViewModel.SelectedIndex = index;
            ViewModel.SelectedX = MarkerCoordinates[0].X = mc_x;
            ViewModel.SelectedY = MarkerCoordinates[0].Y = mc_y;
            Marker.IsVisible = true;

            UpdateInfinity();
        }
        else if (pp.IsRightButtonPressed) // delete
        {
            var co_marker = new Coordinates(
                MarkerCoordinates[0].X / Plot1.ScaleFactor,
                MarkerCoordinates[0].Y / Plot1.ScaleFactor);
            var px_selected = Plot1.GetPixel(co_marker);
            var px_d = px_selected - new Pixel(mp.X, mp.Y);
            var d = double.Sqrt(double.Pow(px_d.X, 2) + double.Pow(px_d.Y, 2));
            if (d > _markerSize) { return; }

            ViewModel.LineDataX.RemoveAt(ViewModel.SelectedIndex);
            ViewModel.LineDataY.RemoveAt(ViewModel.SelectedIndex);

            MarkerCoordinates[0] = Coordinates.NaN;
            Marker.IsVisible = false;

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
        App.MainWindow.PositionChanged -= MainWindow_PositionChanged;
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;
        WinUIPlot1.PointerMoved -= WinUIPlot1_PointerMoved;
        WinUIPlot1.PointerPressed -= WinUIPlot1_PointerPressed;
        WinUIPlot1.PointerReleased -= WinUIPlot1_PointerReleased;

        Bindings.StopTracking();
    }

    private async void ChartCurveEditorDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        while (IsLoaded)
        {
            await Task.Delay(100);
        }
        await Task.Delay(100);

        var points = Chart.Data.GetScatterPoints().Skip(1).SkipLast(1).Select(c => new Point2d(c.X, c.Y)).ToList();
        _messenger.Send<ChartCurveChangedMessage>(new(points));
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
        MarkerCoordinates[0].X = sender.Value = args.NewValue;

        WinUIPlot1.Refresh();
    }

    private void NB_Y_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        MarkerCoordinates[0].Y = sender.Value = args.NewValue;
        UpdateInfinity();

        WinUIPlot1.Refresh();
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
