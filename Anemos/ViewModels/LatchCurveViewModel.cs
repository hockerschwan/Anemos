using System.ComponentModel;
using Anemos.Models;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;

namespace Anemos.ViewModels;

public class LatchCurveViewModel : CurveViewModelBase
{
    private readonly LatchCurveModel _model;

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

    public LatchCurveViewModel(LatchCurveModel model) : base(model)
    {
        _model = model;

        SetLineData();
        LineOutputLow.ItemsSource = _lineDataOutputLow;
        LineOutputHigh.ItemsSource = _lineDataOutputHigh;
        Plot.Series.Insert(0, LineOutputLow);
        Plot.Series.Insert(0, LineOutputHigh);
        Plot.Annotations.Add(ArrowThresholdLow);
        Plot.Annotations.Add(ArrowThresholdHigh);

        SetColor();
    }

    private protected override void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        base.Model_PropertyChanged(sender, e);

        if (e.PropertyName == nameof(_model.OutputLowTemperature) ||
            e.PropertyName == nameof(_model.OutputHighTemperature) ||
            e.PropertyName == nameof(_model.TemperatureThresholdLow) ||
            e.PropertyName == nameof(_model.TemperatureThresholdHigh))
        {
            SetLineData();
            if (IsVisible)
            {
                Plot.InvalidatePlot(true);
            }
        }
    }

    private void SetLineData()
    {
        _lineDataOutputLow.Clear();
        _lineDataOutputLow.Add(new(XAxis.AbsoluteMinimum - 10, _model.OutputLowTemperature));
        _lineDataOutputLow.Add(new(_model.TemperatureThresholdHigh, _model.OutputLowTemperature));

        _lineDataOutputHigh.Clear();
        _lineDataOutputHigh.Add(new(_model.TemperatureThresholdLow, _model.OutputHighTemperature));
        _lineDataOutputHigh.Add(new(XAxis.AbsoluteMaximum + 10, _model.OutputHighTemperature));

        ArrowThresholdLow.StartPoint = new(_model.TemperatureThresholdLow, _model.OutputHighTemperature);
        ArrowThresholdLow.EndPoint = new(_model.TemperatureThresholdLow, _model.OutputLowTemperature);

        ArrowThresholdHigh.StartPoint = new(_model.TemperatureThresholdHigh, _model.OutputLowTemperature);
        ArrowThresholdHigh.EndPoint = new(_model.TemperatureThresholdHigh, _model.OutputHighTemperature);
    }

    private protected override void SetColor()
    {
        LineOutputLow.Color = LineOutputHigh.Color = ArrowThresholdLow.Color = ArrowThresholdHigh.Color
            = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        base.SetColor();
    }
}
