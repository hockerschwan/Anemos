using System.ComponentModel;
using Anemos.Models;
using OxyPlot;
using OxyPlot.Series;

namespace Anemos.ViewModels;

public class ChartCurveViewModel : CurveViewModelBase
{
    private readonly ChartCurveModel _model;

    private readonly List<DataPoint> _lineData = new();

    private readonly LineSeries Line = new()
    {
        StrokeThickness = 2,
        MarkerType = MarkerType.None,
        CanTrackerInterpolatePoints = false,
        Selectable = false,
    };

    public ChartCurveViewModel(ChartCurveModel model) : base(model)
    {
        _model = model;

        SetLineData();
        Line.ItemsSource = _lineData;
        Plot.Series.Insert(0, Line);

        SetColor();
    }

    private protected override void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        base.Model_PropertyChanged(sender, e);

        if (e.PropertyName == nameof(_model.Points))
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
        var dataPoints = new List<DataPoint>();
        if (_model.Points.Any())
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, (double)_model.Points.First().Y));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, (double)_model.Points.Last().Y));
        }
        else
        {
            dataPoints.Add(new(XAxis.AbsoluteMinimum - 10, 0));
            dataPoints.Add(new(XAxis.AbsoluteMaximum + 10, 0));
        }
        dataPoints.InsertRange(1, _model.Points.Select(p => new DataPoint((double)p.X, (double)p.Y)));

        _lineData.Clear();
        _lineData.AddRange(dataPoints);
    }

    private protected override void SetColor()
    {
        Line.Color = OxyColor.Parse(_settingsService.Settings.ChartLineColor);
        base.SetColor();
    }
}
