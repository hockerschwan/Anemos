using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Anemos.ViewModels;

public class CurveEditorViewModel : ObservableRecipient
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set
        {
            SetProperty(ref _id, value);

            OnPropertyChanged(nameof(CurveModel));
            OnPropertyChanged(nameof(CurveModelSource));

            if (CurveModel == null)
            {
                SetLineData(Enumerable.Empty<Point2>());
            }
            else
            {
                SetLineData(CurveModel.Points);
            }

            _scatterData.Clear();
            HoveredIndex = -1;
            _isDragged = false;

            Plot.InvalidatePlot(true);
        }
    }

    public CurveModel? CurveModel => _curveService.GetCurve(Id);

    public SensorModelBase? CurveModelSource => CurveModel?.SourceModel;

    private readonly List<DataPoint> _lineData = new();

    private readonly LimitedObservableCollection<ScatterPoint> _scatterData = new(1);

    private int _hoveredIndex = -1;
    private int HoveredIndex
    {
        get => _hoveredIndex;
        set
        {
            SetProperty(ref _hoveredIndex, value);
            OnPropertyChanged(nameof(IsPointSelected));
            if (IsPointSelected)
            {
                HoveredX = _lineData[HoveredIndex].X;
            }
            else
            {
                HoveredX = double.NaN;
            }
        }
    }

    public bool IsPointSelected => HoveredIndex > 0 && HoveredIndex < _lineData.Count - 1;

    private double _hoveredX;
    public double HoveredX
    {
        get => _hoveredX;
        set
        {
            if (SetProperty(ref _hoveredX, value) && !_isDragged && !double.IsNaN(value))
            {
                var valueOriginal = value + 0d;
                var x = _lineData.ElementAt(HoveredIndex).X;
                var prev = _lineData.Last(p => p.X < x);
                var next = _lineData.First(p => p.X > x);
                value = Math.Max(prev.X + 0.1, Math.Min(next.X - 0.1, value));
                value = Math.Max(XAxis.Minimum, Math.Min(XAxis.Maximum, value));

                SetProperty(ref _hoveredX, value);

                var point = new DataPoint(value, _lineData[HoveredIndex].Y);
                _lineData.RemoveAt(HoveredIndex);
                _lineData.Insert(HoveredIndex, point);

                _scatterData.Add(new(point.X, point.Y));

                Plot.InvalidatePlot(true);
            }
        }
    }

    public double HoveredY
    {
        get
        {
            if (IsPointSelected)
            {
                return _lineData[HoveredIndex].Y;
            }
            return double.NaN;
        }
        set
        {
            if (!_isDragged && HoveredY != value && !double.IsNaN(value))
            {
                value = Math.Max(YAxis.AbsoluteMinimum, Math.Min(YAxis.AbsoluteMaximum, value));

                var point = new DataPoint(_lineData[HoveredIndex].X, value);
                _lineData.RemoveAt(HoveredIndex);
                _lineData.Insert(HoveredIndex, point);
                UpdateInfinity();

                _scatterData.Add(new(point.X, point.Y));

                Plot.InvalidatePlot(true);
            }
        }
    }
    private bool _isDragged;

    public PlotController ChartController = new();

    public PlotModel Plot { get; } = new();

    private readonly LineSeries Line = new()
    {
        StrokeThickness = 2,
        MarkerSize = 5,
        MarkerType = MarkerType.Circle,
        CanTrackerInterpolatePoints = false,
        Selectable = true,
        SelectionMode = SelectionMode.Single,
    };

    private readonly ScatterSeries SelectedScatter = new()
    {
        MarkerSize = 8,
        MarkerType = MarkerType.Circle,
        Selectable = false,
    };

    private readonly LinearAxis XAxis = new()
    {
        Position = AxisPosition.Bottom,
        AbsoluteMaximum = 150,
        AbsoluteMinimum = -273,
        Maximum = 100,
        Minimum = 0,
        IsZoomEnabled = false,
        IsPanEnabled = false,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineThickness = 1,
    };

    private readonly LinearAxis YAxis = new()
    {
        Position = AxisPosition.Left,
        AbsoluteMaximum = 100,
        AbsoluteMinimum = 0,
        Maximum = 100,
        Minimum = 0,
        MaximumDataMargin = 2,
        MinimumDataMargin = 2,
        IsZoomEnabled = false,
        IsPanEnabled = false,
        MajorGridlineStyle = LineStyle.Solid,
        MajorGridlineThickness = 1,
    };

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public CurveEditorViewModel()
    {
        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        ChartController.UnbindAll();
        ChartController.BindMouseEnter(new DelegatePlotCommand<OxyMouseEventArgs>(MouseHoverHandler));
        ChartController.BindMouseDown(OxyMouseButton.Left, new DelegatePlotCommand<OxyMouseDownEventArgs>(MouseDownHandler));
        ChartController.BindMouseDown(OxyMouseButton.Right, new DelegatePlotCommand<OxyMouseDownEventArgs>(MouseRightDownHandler));
#pragma warning disable CS0618 // Type or member is obsolete
        Plot.MouseUp += Plot_MouseUp;
        Plot.MouseMove += Plot_MouseMove;
        Plot.TrackerChanged += Plot_TrackerChanged;
#pragma warning restore CS0618 // Type or member is obsolete

        Line.ItemsSource = _lineData;
        Plot.Series.Add(Line);

        SelectedScatter.ItemsSource = _scatterData;
        Plot.Series.Add(SelectedScatter);

        XAxis.Maximum = _settingsService.Settings.CurveMaxTemp;
        XAxis.Minimum = _settingsService.Settings.CurveMinTemp;
        Plot.Axes.Add(XAxis);
        Plot.Axes.Add(YAxis);

        SetColor();
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.CurveMinTemp))
        {
            XAxis.Minimum = _settingsService.Settings.CurveMinTemp;
        }
        else if (e.PropertyName == nameof(_settingsService.Settings.CurveMaxTemp))
        {
            XAxis.Maximum = _settingsService.Settings.CurveMaxTemp;
        }
        else if (e.PropertyName!.Contains("Chart") && e.PropertyName!.Contains("Color"))
        {
            SetColor();
        }
    }

    private void SetColor()
    {
        Line.Color = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        SelectedScatter.MarkerFill = SelectedScatter.MarkerStroke = OxyColor.Parse(_settingsService.Settings.ChartMarkerColor);
        Plot.Background = OxyColor.Parse(_settingsService.Settings.ChartBGColor);
        XAxis.MajorGridlineColor = YAxis.MajorGridlineColor = XAxis.TicklineColor = YAxis.TicklineColor
            = OxyColor.Parse(_settingsService.Settings.ChartGridColor);
        XAxis.TextColor = YAxis.TextColor = OxyColor.Parse(_settingsService.Settings.ChartTextColor);
        Plot.InvalidatePlot(false);
    }

    private void MouseHoverHandler(IPlotView view, IController controller, OxyMouseEventArgs args)
    {
        args.Handled = true;

        controller.AddHoverManipulator(view, new TrackerManipulator(view)
        {
            FiresDistance = 10,
            LockToInitialSeries = false,
            Snap = false,
            PointsOnly = true
        }, args);
    }

    private void MouseDownHandler(IPlotView view, IController controller, OxyMouseDownEventArgs args)
    {
        _isDragged = true;

        if (args.HitTestResult?.Item == null)
        {
            var mousePos = Line.InverseTransform(args.Position);
            var point = new DataPoint(Math.Round(mousePos.X, 1), Math.Round(mousePos.Y, 1));

            var index = _lineData.FindIndex(p => p.X > point.X);
            _lineData.Insert(index, point);
            HoveredIndex = index;
            UpdateInfinity();

            _scatterData.Add(new(point.X, point.Y));

            Plot.InvalidatePlot(true);
        }
        else
        {
            var pos = (DataPoint)args.HitTestResult.Item;
        }

        OnPropertyChanged(nameof(IsPointSelected));
        OnPropertyChanged(nameof(HoveredY));
    }

    private void MouseRightDownHandler(IPlotView view, IController controller, OxyMouseDownEventArgs args)
    {
        if (args.HitTestResult?.Item == null) { return; }

        _lineData.RemoveAt(HoveredIndex);

        if (_lineData.Count < 3)
        {
            _lineData.Clear();
            _lineData.Add(new(XAxis.AbsoluteMinimum - 10, 0));
            _lineData.Add(new(XAxis.AbsoluteMaximum + 10, 0));
        }
        else if (_lineData.Count == 3)
        {
            UpdateInfinity();
        }
        else if (HoveredIndex == 1)
        {
            UpdateInfinity(-1);
        }
        else if (HoveredIndex == _lineData.Count - 1)
        {
            UpdateInfinity(1);
        }

        HoveredIndex = -1;
        OnPropertyChanged(nameof(IsPointSelected));
        OnPropertyChanged(nameof(HoveredY));
        _scatterData.Clear();

        Plot.InvalidatePlot(true);
    }

    private void Plot_MouseUp(object? sender, OxyMouseEventArgs e)
    {
        _isDragged = false;
    }

    private void Plot_MouseMove(object? sender, OxyMouseEventArgs e)
    {
        if (!_isDragged) { return; }

        var mousePos = Line.InverseTransform(e.Position);
        var x = mousePos.X;
        var y = Math.Max(0, Math.Min(100, mousePos.Y));

        // find neighbours
        var hoveredPoint = _lineData.ElementAt(HoveredIndex);
        var prev = _lineData.Last(p => p.X < hoveredPoint.X);
        var next = _lineData.First(p => p.X > hoveredPoint.X);
        x = Math.Max(prev.X + 0.1, Math.Min(next.X - 0.1, x));
        x = Math.Max(XAxis.Minimum, Math.Min(XAxis.Maximum, x));

        var dp = new DataPoint(Math.Round(x, 1), Math.Round(y, 1));
        _lineData.RemoveAt(HoveredIndex);
        _lineData.Insert(HoveredIndex, dp);
        _scatterData.Add(new(dp.X, dp.Y));

        if (HoveredIndex == 1)
        {
            UpdateInfinity(-1);
        }
        if (HoveredIndex == _lineData.Count - 2)
        {
            UpdateInfinity(1);
        }

        HoveredX = dp.X;
        OnPropertyChanged(nameof(HoveredY));

        Plot.InvalidatePlot(true);
    }

    private void Plot_TrackerChanged(object? sender, TrackerEventArgs e)
    {
        if (_isDragged || e.HitResult == null || e.HitResult.Series is ScatterSeries) { return; }

        var dp = (DataPoint)e.HitResult.Item;
        var index = _lineData.IndexOf(dp);
        if (index == HoveredIndex) { return; }

        HoveredIndex = index;
        OnPropertyChanged(nameof(IsPointSelected));
        OnPropertyChanged(nameof(HoveredY));
        _scatterData.Add(new(dp.X, dp.Y));

        Plot.InvalidatePlot(true);
    }

    private void UpdateInfinity(int end = 0)
    {
        if (end <= 0) // -Inf
        {
            _lineData.RemoveAt(0);
            _lineData.Insert(0, new(XAxis.AbsoluteMinimum - 10, _lineData.First().Y));
        }
        if (end >= 0) // +Inf
        {
            _lineData.RemoveAt(_lineData.Count - 1);
            _lineData.Add(new(XAxis.AbsoluteMaximum + 10, _lineData.Last().Y));
        }
    }

    private void SetLineData(IEnumerable<Point2> points)
    {
        var dataPoints = new List<DataPoint>();
        if (points.Any())
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, (double)points.First().Y));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, (double)points.Last().Y));
        }
        else
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, 0));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, 0));
        }
        dataPoints.InsertRange(1, points.Select(p => new DataPoint((double)p.X, (double)p.Y)));

        _lineData.Clear();
        _lineData.AddRange(dataPoints);
    }

    public IEnumerable<Point2> GetLineData()
    {
        return _lineData.Where(dp => dp.X > XAxis.AbsoluteMinimum && dp.X < XAxis.AbsoluteMaximum)
                        .Select(dp => new Point2(double.Round(dp.X, 1), double.Round(dp.Y, 1)));
    }

    public void Unselect()
    {
        HoveredIndex = -1;
        OnPropertyChanged(nameof(IsPointSelected));
        OnPropertyChanged(nameof(HoveredX));
        OnPropertyChanged(nameof(HoveredY));
    }
}
