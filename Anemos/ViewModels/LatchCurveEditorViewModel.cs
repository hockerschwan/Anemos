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
                LineDataHighTempX[0] = LineDataHighToLowX[0] = LineDataHighToLowX[1] = value;
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
                LineDataLowTempX[1] = LineDataLowToHighX[0] = LineDataLowToHighX[1] = value;
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

    internal readonly double[] LineDataLowToHighX = { 0, 0 };
    internal readonly double[] LineDataLowToHighY = { 0, 0 }; // base, tip

    internal readonly double[] LineDataHighToLowX = { 0, 0 };
    internal readonly double[] LineDataHighToLowY = { 0, 0 }; // base, tip

    public LatchCurveEditorViewModel()
    {
        LineDataLowTempX[0] = LineDataHighTempX[0] = XLowMin - 10d;
        LineDataLowTempX[1] = LineDataHighTempX[1] = XHighMax + 10d;
    }
}
