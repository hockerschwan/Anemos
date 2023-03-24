using System.Collections.ObjectModel;
using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Helpers;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Anemos.ViewModels;

public partial class CurveViewModel : ObservableRecipient
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    public CurveModel Model
    {
        get;
    }

    public RangeObservableCollection<SensorModelBase> Sensors
    {
        get;
    }

    public SensorModelBase? Source
    {
        get => Model.SourceModel;
        set => Model.SourceId = value?.Id ?? string.Empty;
    }

    private SensorModelBase? _oldSource;

    private readonly List<DataPoint> _lineData = new();

    private readonly LimitedObservableCollection<ScatterPoint> _scatterData = new(1);

    public PlotController ChartController = new();

    public PlotModel Plot { get; } = new();

    private readonly LineSeries Line = new()
    {
        StrokeThickness = 2,
        MarkerType = MarkerType.None,
        CanTrackerInterpolatePoints = false,
        Selectable = false,
    };

    private readonly ScatterSeries Scatter = new()
    {
        MarkerSize = 5,
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
        LabelFormatter = EmptyLabelFormatter,
        TickStyle = TickStyle.None,
        AxisTickToLabelDistance = 0,
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
        LabelFormatter = EmptyLabelFormatter,
        TickStyle = TickStyle.None,
        AxisTickToLabelDistance = 0,
    };

    private bool _isVisible = false;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            SetProperty(ref _isVisible, value);
            if (_isVisible)
            {
                SetScatterData();
            }
        }
    }

    public CurveViewModel(CurveModel model)
    {
        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);
        Messenger.Register<CustomSensorsChangedMessage>(this, CustomSensorsChangedMessageHandler);

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Model = model;
        Model.PropertyChanged += Model_PropertyChanged;

        _oldSource = Model.SourceModel;

        if (Model.SourceModel != null)
        {
            Model.SourceModel.PropertyChanged += Source_PropertyChanged;
        }

        Sensors = new(_sensorService.Sensors);

        ChartController.UnbindAll();

        SetLineData();
        Line.ItemsSource = _lineData;
        Plot.Series.Add(Line);

        Scatter.ItemsSource = _scatterData;
        Plot.Series.Add(Scatter);

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

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        Sensors.ReplaceRange(message.NewValue);

        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any() && Model.SourceId != string.Empty && removed.Select(s => s.Id).Contains(Model.SourceId))
        {
            Model.SourceId = string.Empty;
        }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.CurveMaxTemp))
        {
            XAxis.Maximum = _settingsService.Settings.CurveMaxTemp;
            Plot.InvalidatePlot(false);
        }
        else if (e.PropertyName == nameof(_settingsService.Settings.CurveMinTemp))
        {
            XAxis.Minimum = _settingsService.Settings.CurveMinTemp;
            Plot.InvalidatePlot(false);
        }
        else if (e.PropertyName!.Contains("Chart") && e.PropertyName!.Contains("Color"))
        {
            SetColor();
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.SourceId))
        {
            if (_oldSource != null)
            {
                _oldSource!.PropertyChanged -= Source_PropertyChanged;
            }

            if (Model.SourceModel != null)
            {
                Model.SourceModel.PropertyChanged += Source_PropertyChanged;
            }

            _oldSource = Model.SourceModel;

            SetScatterData();
        }
        else if (e.PropertyName == nameof(Model.Value))
        {
            SetScatterData();
        }
        else if (e.PropertyName == nameof(Model.Points))
        {
            SetLineData();
            if (IsVisible)
            {
                Plot.InvalidatePlot(true);
            }
        }
    }

    private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Value")
        {
            SetScatterData();
        }
    }

    private void SetLineData()
    {
        var dataPoints = new List<DataPoint>();
        if (Model.Points.Any())
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, (double)Model.Points.First().Y));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, (double)Model.Points.Last().Y));
        }
        else
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, 0));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, 0));
        }
        dataPoints.InsertRange(1, Model.Points.Select(p => new DataPoint((double)p.X, (double)p.Y)));

        _lineData.Clear();
        _lineData.AddRange(dataPoints);
    }

    private void SetScatterData()
    {
        if (Model.SourceModel?.Value == null || Model.Value == null)
        {
            _scatterData.Clear();
        }
        else
        {
            _scatterData.Add(new((double)Model.SourceModel.Value, (double)Model.Value));
        }

        if (IsVisible)
        {
            Plot.InvalidatePlot(true);
        }
    }

    private void SetColor()
    {
        Line.Color = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        Scatter.MarkerFill = Scatter.MarkerStroke = OxyColor.Parse(_settingsService.Settings.ChartMarkerColor);
        Plot.Background = OxyColor.Parse(_settingsService.Settings.ChartBGColor);
        XAxis.MajorGridlineColor = YAxis.MajorGridlineColor = XAxis.TicklineColor = YAxis.TicklineColor
            = OxyColor.Parse(_settingsService.Settings.ChartGridColor);
        XAxis.TextColor = YAxis.TextColor = OxyColor.Parse(_settingsService.Settings.ChartTextColor);
    }

    private static string EmptyLabelFormatter(double arg) => string.Empty;

    [RelayCommand]
    private void RemoveSelf()
    {
        Messenger.UnregisterAll(this);
        Model.PropertyChanged -= Model_PropertyChanged;
        if (_oldSource != null)
        {
            _oldSource.PropertyChanged -= Source_PropertyChanged;
            _oldSource = null;
        }

        _curveService.RemoveCurve(Model.Id);
    }
}
