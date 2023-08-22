using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class LatchCurveEditorViewModel : ObservableObject
{
    internal double MinTemperature => ICurveService.AbsoluteMinTemperature;
    internal double MaxTemperature => ICurveService.AbsoluteMaxTemperature;

    private double _temperatureThresholdLow = double.NaN;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (value >= TemperatureThresholdHigh)
            {
                value = TemperatureThresholdHigh - 1d;
            }

            if (SetProperty(ref _temperatureThresholdLow, value))
            {
                LineDataHighTempX[0] = LineDataHighToLowX[0] = LineDataHighToLowX[1] = TemperatureThresholdLow;
            }
        }
    }

    private double _temperatureThresholdHigh = double.NaN;
    public double TemperatureThresholdHigh
    {
        get => _temperatureThresholdHigh;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (value <= TemperatureThresholdLow)
            {
                value = TemperatureThresholdLow + 1d;
            }

            if (SetProperty(ref _temperatureThresholdHigh, value))
            {
                LineDataLowTempX[1] = LineDataLowToHighX[0] = LineDataLowToHighX[1] = TemperatureThresholdHigh;
            }
        }
    }

    private double _outputLowTemperature = double.NaN;
    public double OutputLowTemperature
    {
        get => _outputLowTemperature;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (SetProperty(ref _outputLowTemperature, value))
            {
                LineDataLowTempY[0] = LineDataLowTempY[1] = LineDataLowToHighY[0] = LineDataHighToLowY[1] = OutputLowTemperature;
            }
        }
    }

    private double _outputHighTemperature = double.NaN;
    public double OutputHighTemperature
    {
        get => _outputHighTemperature;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (SetProperty(ref _outputHighTemperature, value))
            {
                LineDataHighTempY[0] = LineDataHighTempY[1] = LineDataLowToHighY[1] = LineDataHighToLowY[0] = OutputHighTemperature;
            }
        }
    }

    internal readonly double[] LineDataLowTempX = { 0, 0 };
    internal readonly double[] LineDataLowTempY = { 0, 0 };

    internal readonly double[] LineDataHighTempX = { 0, 0 };
    internal readonly double[] LineDataHighTempY = { 0, 0 };

    internal readonly double[] LineDataLowToHighX = { 0, 0 };
    internal readonly double[] LineDataLowToHighY = { 0, 0 }; // base, tip

    internal readonly double[] LineDataHighToLowX = { 0, 0 };
    internal readonly double[] LineDataHighToLowY = { 0, 0 }; // base, tip

    public LatchCurveEditorViewModel()
    {
        LineDataLowTempX[0] = LineDataHighTempX[0] = MinTemperature;
        LineDataLowTempX[1] = LineDataHighTempX[1] = MaxTemperature;
    }
}
