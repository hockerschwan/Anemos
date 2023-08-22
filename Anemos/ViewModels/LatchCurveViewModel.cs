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

    internal readonly double[] LineDataLowTempX = { 0, 0 };
    internal readonly double[] LineDataLowTempY = { 0, 0 };

    internal readonly double[] LineDataHighTempX = { 0, 0 };
    internal readonly double[] LineDataHighTempY = { 0, 0 };

    internal readonly double[] LineDataLowToHighX = { 0, 0 };
    internal readonly double[] LineDataLowToHighY = { 0, 0 }; // base, tip

    internal readonly double[] LineDataHighToLowX = { 0, 0 };
    internal readonly double[] LineDataHighToLowY = { 0, 0 }; // base, tip

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
        LineDataLowTempX[1] = LineDataLowToHighX[0] = LineDataLowToHighX[1] = CurveModel.TemperatureThresholdHigh;
        LineDataLowTempY[0] = LineDataLowTempY[1] = LineDataLowToHighY[0] = LineDataHighToLowY[1] = CurveModel.OutputLowTemperature;
        LineDataHighTempX[0] = LineDataHighToLowX[0] = LineDataHighToLowX[1] = CurveModel.TemperatureThresholdLow;
        LineDataHighTempY[0] = LineDataHighTempY[1] = LineDataLowToHighY[1] = LineDataHighToLowY[0] = CurveModel.OutputHighTemperature;

        OnCurveDataChanged();
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        SetLineData();
        _curveService.Save();
    }
}
