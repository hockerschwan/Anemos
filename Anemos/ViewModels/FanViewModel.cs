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

public partial class FanViewModel : ObservableRecipient
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private readonly ICurveService _curveService = App.GetService<ICurveService>();

    private static FansViewModel FansVM => App.GetService<FansViewModel>();

    public FanModelBase Model
    {
        get;
    }

    public bool ShowConstantControls => Model.ControlMode == FanControlModes.Constant;

    public bool ShowCurveControls => Model.ControlMode == FanControlModes.Curve;

    public string[] ControlModeNames
    {
        get;
    } = new[]
    {
        "Fan_ControlMode_Device".GetLocalized(),
        "Fan_ControlMode_Constant".GetLocalized(),
        "Fan_ControlMode_Curve".GetLocalized()
    };

    private int _controlModeIndex = -1;
    public int ControlModeIndex
    {
        get => _controlModeIndex;
        set
        {
            if (SetProperty(ref _controlModeIndex, value))
            {
                Model.ControlMode = (FanControlModes)_controlModeIndex;
                OnPropertyChanged(nameof(ShowConstantControls));
                OnPropertyChanged(nameof(ShowCurveControls));
            }
        }
    }

    public RangeObservableCollection<CurveModel> Curves
    {
        get;
    }

    private CurveModel? _selectedCurve;
    public CurveModel? SelectedCurve
    {
        get => _selectedCurve;
        set
        {
            if (SetProperty(ref _selectedCurve, value))
            {
                Model.CurveId = _selectedCurve?.Id ?? string.Empty;
            }
        }
    }

    private readonly LimitedQueue<DataPoint> _lineData = new(120);

    public PlotController ChartController = new();

    public PlotModel Plot { get; } = new();

    private readonly LineSeries Line = new()
    {
        StrokeThickness = 2,
        MarkerType = MarkerType.None,
        CanTrackerInterpolatePoints = false,
        Selectable = false,
    };

    private readonly LinearAxis XAxis = new()
    {
        Position = AxisPosition.Bottom,
        MaximumRange = 60,
        MinimumRange = 60,
        IsAxisVisible = false,
        IsZoomEnabled = false,
        IsPanEnabled = false,
    };

    private readonly LinearAxis YAxis = new()
    {
        Position = AxisPosition.Left,
        MinimumRange = 100,
        IsZoomEnabled = false,
        IsPanEnabled = false,
        MajorGridlineStyle = LineStyle.None,
    };

    public FanViewModel(FanModelBase model)
    {
        Messenger.Register<CurvesChangedMessage>(this, CurvesChangedMessageHandler);
        Messenger.Register<FanProfileChangedMessage>(this, FanProfileChangedMessageHandler);

        Model = model;
        Model.PropertyChanged += Model_PropertyChanged;

        _settingsService.Settings.PropertyChanged += Settings_PropertyChanged;

        Curves = new(_curveService.Curves);

        _controlModeIndex = (int)Model.ControlMode;
        _selectedCurve = Model.CurveModel;

        ChartController.UnbindAll();

        XAxis.MaximumRange = XAxis.MinimumRange = _lineData.Limit = _settingsService.Settings.FanHistory;
        Line.MinimumSegmentLength = Math.Max(2, _lineData.Limit / 100);

        Line.ItemsSource = _lineData;
        Plot.Series.Add(Line);

        Plot.Axes.Add(XAxis);
        Plot.Axes.Add(YAxis);

        SetColor();
    }

    private void CurvesChangedMessageHandler(object recipient, CurvesChangedMessage message)
    {
        Curves.ReplaceRange(message.NewValue);

        var removed = message.OldValue.Except(message.NewValue).ToList();
        if (removed.Any() && Model.CurveId != string.Empty && removed.Select(cm => cm.Id).Contains(Model.CurveId))
        {
            Model.CurveId = string.Empty;
        }
    }

    private void FanProfileChangedMessageHandler(object recipient, FanProfileChangedMessage message)
    {
        _controlModeIndex = (int)Model.ControlMode;
        _selectedCurve = Model.CurveModel;

        OnPropertyChanged(nameof(ControlModeIndex));
        OnPropertyChanged(nameof(SelectedCurve));
        OnPropertyChanged(nameof(ShowConstantControls));
        OnPropertyChanged(nameof(ShowCurveControls));
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Model.CurrentRPM))
        {
            if (Model.CurrentRPM != null)
            {
                if (_lineData.Any())
                {
                    _lineData.Enqueue(new(_lineData.Last().X + 1, (double)Model.CurrentRPM));
                }
                else
                {
                    _lineData.Enqueue(new(0, (double)Model.CurrentRPM));
                }
            }

            if (FansVM.IsVisible)
            {
                Plot.InvalidatePlot(true);
            }
        }
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_settingsService.Settings.FanHistory))
        {
            XAxis.MaximumRange = XAxis.MinimumRange = _lineData.Limit = _settingsService.Settings.FanHistory;
            Line.MinimumSegmentLength = Math.Max(2, _lineData.Limit / 100);
            Plot.InvalidatePlot(false);
        }
        else if (e.PropertyName!.Contains("Chart") && e.PropertyName!.Contains("Color"))
        {
            SetColor();
        }
    }

    private void SetColor()
    {
        Line.Color = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        Plot.Background = OxyColor.Parse(_settingsService.Settings.ChartBGColor);
        XAxis.TicklineColor = YAxis.TicklineColor = OxyColor.Parse(_settingsService.Settings.ChartGridColor);
        XAxis.TextColor = YAxis.TextColor = OxyColor.Parse(_settingsService.Settings.ChartTextColor);
    }

    [RelayCommand]
    private void OpenOptions()
    {
        Messenger.Send(new OpenFanOptionsMessage(Model.Id));
    }
}
