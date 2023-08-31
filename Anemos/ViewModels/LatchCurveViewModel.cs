using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;
using ScottPlot;

namespace Anemos.ViewModels;

internal class LatchCurveViewModel : CurveViewModelBase
{
    private LatchCurveModel CurveModel
    {
        get;
    }

    internal readonly double[] LineDataLowTempX = { 0, 0 };
    internal readonly double[] LineDataLowTempY = { 0, 0 };

    internal readonly double[] LineDataHighTempX = { 0, 0 };
    internal readonly double[] LineDataHighTempY = { 0, 0 };

    internal readonly Coordinates[] ArrowLowCoordinates = Enumerable.Repeat(Coordinates.Origin, 2).ToArray();
    internal readonly Coordinates[] ArrowHighCoordinates = Enumerable.Repeat(Coordinates.Origin, 2).ToArray();

    private readonly System.Timers.Timer _timer = new(100) { AutoReset = false };

    public LatchCurveViewModel(LatchCurveModel model) : base(model)
    {
        CurveModel = model;

        LineDataLowTempX[0] = LineDataHighTempX[0] = ICurveService.AbsoluteMinTemperature;
        LineDataLowTempX[1] = LineDataHighTempX[1] = ICurveService.AbsoluteMaxTemperature;

        SetLineData();

        _timer.Elapsed += Timer_Elapsed;
    }

    internal override void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        base.Model_PropertyChanged(sender, e);

        if (e.PropertyName == nameof(CurveModel.OutputLowTemperature) ||
            e.PropertyName == nameof(CurveModel.OutputHighTemperature) ||
            e.PropertyName == nameof(CurveModel.TemperatureThresholdLow) ||
            e.PropertyName == nameof(CurveModel.TemperatureThresholdHigh))
        {
            _timer.Stop();
            _timer.Start();
        }
    }

    private void SetLineData()
    {
        LineDataLowTempX[1] = ArrowHighCoordinates[0].X = ArrowHighCoordinates[1].X = CurveModel.TemperatureThresholdHigh;
        LineDataLowTempY[0] = LineDataLowTempY[1] = ArrowLowCoordinates[1].Y = ArrowHighCoordinates[0].Y = CurveModel.OutputLowTemperature;
        LineDataHighTempX[0] = ArrowLowCoordinates[0].X = ArrowLowCoordinates[1].X = CurveModel.TemperatureThresholdLow;
        LineDataHighTempY[0] = LineDataHighTempY[1] = ArrowLowCoordinates[0].Y = ArrowHighCoordinates[1].Y = CurveModel.OutputHighTemperature;

        OnCurveDataChanged();
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        SetLineData();
        _curveService.Save();
    }
}
