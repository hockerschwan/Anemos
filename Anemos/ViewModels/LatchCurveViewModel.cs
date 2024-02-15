using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;

namespace Anemos.ViewModels;

internal class LatchCurveViewModel : CurveViewModelBase
{
    private LatchCurveModel CurveModel
    {
        get;
    }

    internal readonly double[] LineDataLowTempX = [0, 0];
    internal readonly double[] LineDataLowTempY = [0, 0];

    internal readonly double[] LineDataHighTempX = [0, 0];
    internal readonly double[] LineDataHighTempY = [0, 0];

    internal readonly double[] ArrowLowX = [0, 0];
    internal readonly double[] ArrowLowY = [0, 0];
    internal readonly double[] ArrowHighX = [0, 0];
    internal readonly double[] ArrowHighY = [0, 0];

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
        LineDataLowTempX[1] = ArrowHighX[0] = ArrowHighX[1] = CurveModel.TemperatureThresholdHigh;
        LineDataLowTempY[0] = LineDataLowTempY[1] = ArrowLowY[1] = ArrowHighY[0] = CurveModel.OutputLowTemperature;
        LineDataHighTempX[0] = ArrowLowX[0] = ArrowLowX[1] = CurveModel.TemperatureThresholdLow;
        LineDataHighTempY[0] = LineDataHighTempY[1] = ArrowLowY[0] = ArrowHighY[1] = CurveModel.OutputHighTemperature;

        OnCurveDataChanged();
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        SetLineData();
        _curveService.Save();
    }
}
