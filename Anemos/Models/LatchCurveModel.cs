namespace Anemos.Models;

public class LatchCurveModel(CurveArg args) : CurveModelBase(args)
{
    private double _outputLowTemperature = args.OutputLowTemperature!.Value;
    public double OutputLowTemperature
    {
        get => _outputLowTemperature;
        set => SetProperty(ref _outputLowTemperature, value);
    }

    private double _outputHighTemperature = args.OutputHighTemperature!.Value;
    public double OutputHighTemperature
    {
        get => _outputHighTemperature;
        set => SetProperty(ref _outputHighTemperature, value);
    }

    private double _temperatureThresholdLow = args.TemperatureThresholdLow!.Value;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set => SetProperty(ref _temperatureThresholdLow, value);
    }

    private double _temperatureThresholdHigh = args.TemperatureThresholdHigh!.Value;
    public double TemperatureThresholdHigh
    {
        get => _temperatureThresholdHigh;
        set => SetProperty(ref _temperatureThresholdHigh, value);
    }

    private bool _useHigh = false;

    protected override void Update_()
    {
        Input = SourceModel?.Value;
        Output = CalcValue();
    }

    private double? CalcValue()
    {
        if (SourceModel == null || SourceModel.Value == null)
        {
            return null;
        }

        var temperature = SourceModel.Value;
        if (!temperature.HasValue)
        {
            return null;
        }

        if (_useHigh && temperature <= TemperatureThresholdLow)
        {
            _useHigh = false;
        }
        else if (!_useHigh && temperature >= TemperatureThresholdHigh)
        {
            _useHigh = true;
        }

        return _useHigh ? OutputHighTemperature : OutputLowTemperature;
    }
}
