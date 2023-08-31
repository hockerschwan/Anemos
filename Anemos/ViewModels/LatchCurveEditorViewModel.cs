using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;

namespace Anemos.ViewModels;

public class LatchCurveEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    private double _temperatureThresholdLow = double.NaN;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (SetProperty(ref _temperatureThresholdLow, value))
            {
                LineDataHighTempX[0] = ArrowLowCoordinates[0].X = ArrowLowCoordinates[1].X = value;
                XHighMin = value + 1d;
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

            if (SetProperty(ref _temperatureThresholdHigh, value))
            {
                LineDataLowTempX[1] = ArrowHighCoordinates[0].X = ArrowHighCoordinates[1].X = value;
                XLowMax = value - 1d;
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
                LineDataLowTempY[0] = LineDataLowTempY[1]
                    = ArrowLowCoordinates[1].Y = ArrowHighCoordinates[0].Y = OutputLowTemperature;
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
                LineDataHighTempY[0] = LineDataHighTempY[1]
                    = ArrowLowCoordinates[0].Y = ArrowHighCoordinates[1].Y = OutputHighTemperature;
            }
        }
    }

    public double XLowMin => _settingsService.Settings.CurveMinTemp;

    private double _xLowMax;
    public double XLowMax
    {
        get => _xLowMax;
        set => SetProperty(ref _xLowMax, value);
    }

    private double _xHighMin;
    public double XHighMin
    {
        get => _xHighMin;
        set => SetProperty(ref _xHighMin, value);
    }

    public double XHighMax => _settingsService.Settings.CurveMaxTemp;

    internal readonly double[] LineDataLowTempX = { 0, 0 };
    internal readonly double[] LineDataLowTempY = { 0, 0 };

    internal readonly double[] LineDataHighTempX = { 0, 0 };
    internal readonly double[] LineDataHighTempY = { 0, 0 };

    internal readonly Coordinates[] ArrowLowCoordinates = Enumerable.Repeat(Coordinates.Origin, 2).ToArray();
    internal readonly Coordinates[] ArrowHighCoordinates = Enumerable.Repeat(Coordinates.Origin, 2).ToArray();

    public LatchCurveEditorViewModel()
    {
        LineDataLowTempX[0] = LineDataHighTempX[0] = XLowMin - 10d;
        LineDataLowTempX[1] = LineDataHighTempX[1] = XHighMax + 10d;
    }
}
