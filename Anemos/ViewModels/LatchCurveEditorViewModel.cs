using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

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
                LineDataHighTempX[0] = ArrowLowX[0] = ArrowLowX[1] = value;
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
                LineDataLowTempX[1] = ArrowHighX[0] = ArrowHighX[1] = value;
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
                LineDataLowTempY[0] = LineDataLowTempY[1] = ArrowLowY[1] = ArrowHighY[0] = OutputLowTemperature;
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
                LineDataHighTempY[0] = LineDataHighTempY[1] = ArrowLowY[0] = ArrowHighY[1] = OutputHighTemperature;
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

    internal readonly double[] LineDataLowTempX = [0, 0];
    internal readonly double[] LineDataLowTempY = [0, 0];

    internal readonly double[] LineDataHighTempX = [0, 0];
    internal readonly double[] LineDataHighTempY = [0, 0];

    internal readonly double[] ArrowLowX = [0, 0];
    internal readonly double[] ArrowLowY = [0, 0];
    internal readonly double[] ArrowHighX = [0, 0];
    internal readonly double[] ArrowHighY = [0, 0];

    public LatchCurveEditorViewModel()
    {
        LineDataLowTempX[0] = LineDataHighTempX[0] = XLowMin - 10d;
        LineDataLowTempX[1] = LineDataHighTempX[1] = XHighMax + 10d;
    }
}
