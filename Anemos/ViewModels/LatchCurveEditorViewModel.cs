using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Anemos.ViewModels;

public class LatchCurveEditorViewModel : ObservableRecipient
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

            _outputLowTemperature = CurveModel?.OutputLowTemperature ?? 0;
            _outputHighTemperature = CurveModel?.OutputHighTemperature ?? 0;
            _temperatureThresholdLow = CurveModel?.TemperatureThresholdLow ?? 0;
            _temperatureThresholdHigh = CurveModel?.TemperatureThresholdHigh ?? 0;
            OnPropertyChanged(nameof(OutputLowTemperature));
            OnPropertyChanged(nameof(OutputHighTemperature));
            OnPropertyChanged(nameof(TemperatureThresholdLow));
            OnPropertyChanged(nameof(TemperatureThresholdHigh));

            SetLineData();
        }
    }

    public LatchCurveModel? CurveModel => (LatchCurveModel?)_curveService.GetCurve(Id);

    public SensorModelBase? CurveModelSource => CurveModel?.SourceModel;

    private double _outputLowTemperature;
    public double OutputLowTemperature
    {
        get => _outputLowTemperature;
        set
        {
            if (double.IsNaN(value))
            {
                value = _outputLowTemperature;
            }

            value = Math.Max(YAxis.AbsoluteMinimum, Math.Min(YAxis.AbsoluteMaximum, value));
            if (SetProperty(ref _outputLowTemperature, value))
            {
                SetLineData();
            }
        }
    }

    private double _outputHighTemperature;
    public double OutputHighTemperature
    {
        get => _outputHighTemperature;
        set
        {
            if (double.IsNaN(value))
            {
                value = _outputHighTemperature;
            }

            value = Math.Max(YAxis.AbsoluteMinimum, Math.Min(YAxis.AbsoluteMaximum, value));
            if (SetProperty(ref _outputHighTemperature, value))
            {
                SetLineData();
            }
        }
    }

    private double _temperatureThresholdLow;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set
        {
            if (double.IsNaN(value))
            {
                value = _temperatureThresholdLow;
            }

            if (value >= _temperatureThresholdHigh)
            {
                value = _temperatureThresholdHigh - 0.1;
            }

            value = Math.Max(XAxis.Minimum, Math.Min(XAxis.Maximum, value));
            if (SetProperty(ref _temperatureThresholdLow, value))
            {
                SetLineData();
            }
        }
    }

    private double _temperatureThresholdHigh;
    public double TemperatureThresholdHigh
    {
        get => _temperatureThresholdHigh;
        set
        {
            if (double.IsNaN(value))
            {
                value = _temperatureThresholdHigh;
            }

            if (value <= _temperatureThresholdLow)
            {
                value = _temperatureThresholdLow + 0.1;
            }

            value = Math.Max(XAxis.Minimum, Math.Min(XAxis.Maximum, value));
            if (SetProperty(ref _temperatureThresholdHigh, value))
            {
                SetLineData();
            }
        }
    }

    private readonly List<DataPoint> _lineDataOutputLow = new();
    private readonly List<DataPoint> _lineDataOutputHigh = new();

    private readonly LineSeries LineOutputLow = new()
    {
        StrokeThickness = 2,
        MarkerType = MarkerType.None,
        CanTrackerInterpolatePoints = false,
        Selectable = false,
    };

    private readonly LineSeries LineOutputHigh = new()
    {
        StrokeThickness = 2,
        MarkerType = MarkerType.None,
        CanTrackerInterpolatePoints = false,
        Selectable = false,
    };

    internal readonly ArrowAnnotation ArrowThresholdLow = new() { Selectable = false };
    internal readonly ArrowAnnotation ArrowThresholdHigh = new() { Selectable = false };

    public PlotController ChartController = new();

    public PlotModel Plot { get; } = new();

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

    public LatchCurveEditorViewModel()
    {
        Messenger.Register<WindowVisibilityChangedMessage>(this, WindowVisibilityChangedMessageHandler);

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        ChartController.UnbindAll();

        LineOutputLow.ItemsSource = _lineDataOutputLow;
        LineOutputHigh.ItemsSource = _lineDataOutputHigh;
        Plot.Series.Add(LineOutputLow);
        Plot.Series.Add(LineOutputHigh);
        Plot.Annotations.Add(ArrowThresholdLow);
        Plot.Annotations.Add(ArrowThresholdHigh);

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

    private void SetLineData()
    {
        _lineDataOutputLow.Clear();
        _lineDataOutputHigh.Clear();

        if (CurveModel != null)
        {
            _lineDataOutputLow.Add(new(XAxis.AbsoluteMinimum - 10, OutputLowTemperature));
            _lineDataOutputLow.Add(new(TemperatureThresholdHigh, OutputLowTemperature));

            _lineDataOutputHigh.Add(new(TemperatureThresholdLow, OutputHighTemperature));
            _lineDataOutputHigh.Add(new(XAxis.AbsoluteMaximum + 10, OutputHighTemperature));

            ArrowThresholdLow.StartPoint = new(TemperatureThresholdLow, OutputHighTemperature);
            ArrowThresholdLow.EndPoint = new(TemperatureThresholdLow, OutputLowTemperature);

            ArrowThresholdHigh.StartPoint = new(TemperatureThresholdHigh, OutputLowTemperature);
            ArrowThresholdHigh.EndPoint = new(TemperatureThresholdHigh, OutputHighTemperature);
        }
    }

    private void SetColor()
    {
        LineOutputLow.Color = LineOutputHigh.Color = ArrowThresholdLow.Color = ArrowThresholdHigh.Color
            = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        Plot.Background = OxyColor.Parse(_settingsService.Settings.ChartBGColor);
        XAxis.MajorGridlineColor = YAxis.MajorGridlineColor = XAxis.TicklineColor = YAxis.TicklineColor
            = OxyColor.Parse(_settingsService.Settings.ChartGridColor);
        XAxis.TextColor = YAxis.TextColor = OxyColor.Parse(_settingsService.Settings.ChartTextColor);
        Plot.InvalidatePlot(false);
    }
}
