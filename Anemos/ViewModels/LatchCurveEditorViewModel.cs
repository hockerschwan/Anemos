using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using ScottPlot;

namespace Anemos.ViewModels;

public class LatchCurveEditorEventArgs : EventArgs
{
    public enum Values
    {
        XLow, XHigh, YLow, YHigh
    }

    public Values Property
    {
        get; init;
    }
}

public delegate void LatchCurveEditorPropertyChangedEventHandler(object? sender, LatchCurveEditorEventArgs e);

public class LatchCurveEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    public event LatchCurveEditorPropertyChangedEventHandler? LatchCurveEditorPropertyChanged;

    private double _temperatureThresholdLow = double.NaN;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set
        {
            if (double.IsNaN(value)) { return; }

            if (SetProperty(ref _temperatureThresholdLow, value))
            {
                LineDataHighTempX[0] = ArrowLowBase.X = ArrowLowTip.X = value;
                XHighMin = value + 1d;

                LatchCurveEditorPropertyChanged?.Invoke(this, new()
                {
                    Property = LatchCurveEditorEventArgs.Values.XLow
                });
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
                LineDataLowTempX[1] = ArrowHighBase.X = ArrowHighTip.X = value;
                XLowMax = value - 1d;

                LatchCurveEditorPropertyChanged?.Invoke(this, new()
                {
                    Property = LatchCurveEditorEventArgs.Values.XHigh
                });
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
                LineDataLowTempY[0] = LineDataLowTempY[1] = ArrowLowTip.Y = ArrowHighBase.Y = OutputLowTemperature;

                LatchCurveEditorPropertyChanged?.Invoke(this, new()
                {
                    Property = LatchCurveEditorEventArgs.Values.YLow
                });
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
                LineDataHighTempY[0] = LineDataHighTempY[1] = ArrowLowBase.Y = ArrowHighTip.Y = OutputHighTemperature;

                LatchCurveEditorPropertyChanged?.Invoke(this, new()
                {
                    Property = LatchCurveEditorEventArgs.Values.YHigh
                });
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

    internal Coordinates ArrowLowBase = Coordinates.Origin;
    internal Coordinates ArrowLowTip = Coordinates.Origin;
    internal Coordinates ArrowHighBase = Coordinates.Origin;
    internal Coordinates ArrowHighTip = Coordinates.Origin;

    public LatchCurveEditorViewModel()
    {
        LineDataLowTempX[0] = LineDataHighTempX[0] = XLowMin - 10d;
        LineDataLowTempX[1] = LineDataHighTempX[1] = XHighMax + 10d;
    }
}
