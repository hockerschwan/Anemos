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

public partial class CurveViewModelBase : ObservableRecipient
{
    private protected readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private protected readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private protected readonly ICurveService _curveService = App.GetService<ICurveService>();

    public CurveModelBase Model
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

    private readonly LimitedObservableCollection<ScatterPoint> _scatterData = new(1);

    public PlotController ChartController = new();

    public PlotModel Plot { get; } = new();

    private readonly ScatterSeries Scatter = new()
    {
        MarkerSize = 5,
        MarkerType = MarkerType.Circle,
        Selectable = false,
    };

    private protected readonly LinearAxis XAxis = new()
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

    public CurveViewModelBase(CurveModelBase model)
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

        Scatter.ItemsSource = _scatterData;
        Plot.Series.Add(Scatter);

        XAxis.Maximum = _settingsService.Settings.CurveMaxTemp;
        XAxis.Minimum = _settingsService.Settings.CurveMinTemp;
        Plot.Axes.Add(XAxis);
        Plot.Axes.Add(YAxis);
    }

    private void WindowVisibilityChangedMessageHandler(object recipient, WindowVisibilityChangedMessage message)
    {
        IsVisible = message.Value;
    }

    private void CustomSensorsChangedMessageHandler(object recipient, CustomSensorsChangedMessage message)
    {
        var removed = message.OldValue.Except(message.NewValue).ToList();
        foreach (var sensor in removed)
        {
            Sensors.Remove(sensor);
        }

        var added = message.NewValue.Except(message.OldValue).ToList();
        Sensors.AddRange(added);
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

    private protected virtual void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
    }

    private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Value")
        {
            SetScatterData();
        }
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

    private protected virtual void SetColor()
    {
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

    [RelayCommand]
    private void OpenEditor()
    {
        Messenger.Send(new OpenCurveEditorMessage(Model.Id));
    }
}
