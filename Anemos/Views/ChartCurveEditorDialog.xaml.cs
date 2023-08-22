using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ScottPlot;
using ScottPlot.Control;
using ScottPlot.DataSources;
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
    private readonly Coordinates[] MarkerCoordinates = new[] { Coordinates.NaN };

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
        Closed += ChartCurveEditorDialog_Closed;
        Loaded += ChartCurveEditorDialog_Loaded;
        App.MainWindow.SizeChanged += MainWindow_SizeChanged;

        SetNumberFormatter();

        WinUIPlot1.Interaction.Actions = PlotActions.NonInteractive();
        WinUIPlot1.PointerMoved += WinUIPlot1_PointerMoved;
        WinUIPlot1.PointerPressed += WinUIPlot1_PointerPressed;
        WinUIPlot1.PointerReleased += WinUIPlot1_PointerReleased;

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

        Chart = Plot1.Add.Scatter(ViewModel.LineDataX, ViewModel.LineDataY, LineColor);
        Chart.LineStyle.Width = 2;
        Chart.MarkerStyle.Size = _markerSize;

        Marker = Plot1.Add.Scatter(new ScatterSourceCoordinates(MarkerCoordinates));
        Marker.MarkerStyle.Size = _markerSize * 2;
        Marker.MarkerStyle.Fill.Color = MarkerColor;

        WinUIPlot1.Refresh();
    }

    private Coordinates GetNearestCoordinatesInRange(double x, double y)
    {
        var px = new Pixel(x, y);
        var mc = WinUIPlot1.GetCoordinates(px);

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

        var nearest = near.OrderBy(c => c.Distance(mc)).First();
        var px_d = Plot1.GetPixel(nearest) - px;
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
            var prev = ViewModel.LineDataX.LastOrDefault(x => x < ViewModel.SelectedX, double.NegativeInfinity);
            var next = ViewModel.LineDataX.FirstOrDefault(x => x > ViewModel.SelectedX, double.PositiveInfinity);

            var axisLimits = Plot1.GetAxisLimits();
            var low = double.Max(axisLimits.XRange.Min, prev + 0.1);
            var high = double.Min(axisLimits.XRange.Max, next - 0.1);

            var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;
            var mc = WinUIPlot1.GetCoordinates(new Pixel(mp.X, mp.Y));

            var mc_x = double.Round(double.Max(low, double.Min(high, mc.X)), 1);
            var mc_y = double.Round(double.Max(0d, double.Min(100d, mc.Y)), 1);

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
        }

        WinUIPlot1.Refresh();
    }

    private void WinUIPlot1_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pp = e.GetCurrentPoint(this).Properties;
        if (pp.IsLeftButtonPressed) // add
        {
            _isDragged = true;

            var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;
            var nearest = GetNearestCoordinatesInRange(mp.X, mp.Y);
            if (nearest != Coordinates.NaN) { return; }

            var px = new Pixel(mp.X, mp.Y);
            var mc = WinUIPlot1.GetCoordinates(px);

            var mc_x = double.Round(mc.X, 1);
            var mc_y = double.Round(mc.Y, 1);

            var index = ViewModel.LineDataX.FindIndex(x => x > mc_x);
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
            var mp = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender).Position;
            var nearest = GetNearestCoordinatesInRange(mp.X, mp.Y);
            if (nearest == Coordinates.NaN) { return; }

            var i = ViewModel.LineDataX.IndexOf(nearest.X);
            ViewModel.LineDataX.RemoveAt(i);
            ViewModel.LineDataY.RemoveAt(i);

            MarkerCoordinates[0] = Coordinates.NaN;
            Marker.IsVisible = false;

            ViewModel.SelectedX = ViewModel.SelectedY = double.NaN;
            ViewModel.SelectedIndex = -1;
            UpdateInfinity(true);
        }

        WinUIPlot1.Refresh();
    }

    private void WinUIPlot1_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragged = false;
    }

    private void ChartCurveEditorDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        App.MainWindow.SizeChanged -= MainWindow_SizeChanged;

        var points = Chart.Data.GetScatterPoints().Skip(1).SkipLast(1).Select(c => new Point2d(c.X, c.Y));
        App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(100);
            _messenger.Send<ChartCurveChangedMessage>(new(points));
        });
    }

    private void ChartCurveEditorDialog_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Loaded -= ChartCurveEditorDialog_Loaded;
        SetDialogSize();
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
        if (double.IsNaN(args.NewValue)) { return; }

        var prev = ViewModel.LineDataX.LastOrDefault(x => x < ViewModel.SelectedX, double.NegativeInfinity);
        var next = ViewModel.LineDataX.FirstOrDefault(x => x > ViewModel.SelectedX, double.PositiveInfinity);

        var axisLimits = Plot1.GetAxisLimits();
        var low = double.Max(axisLimits.XRange.Min, prev + 0.1);
        var high = double.Min(axisLimits.XRange.Max, next - 0.1);

        var x = double.Round(double.Max(low, double.Min(high, sender.Value)), 1);
        ViewModel.SelectedX = ViewModel.LineDataX[ViewModel.SelectedIndex] = MarkerCoordinates[0].X = sender.Value = x;

        WinUIPlot1.Refresh();
    }

    private void NB_Y_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (double.IsNaN(args.NewValue)) { return; }

        ViewModel.SelectedY = ViewModel.LineDataY[ViewModel.SelectedIndex] = MarkerCoordinates[0].Y = sender.Value
            = double.Round(double.Max(0d, double.Min(100d, sender.Value)), 1);

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
