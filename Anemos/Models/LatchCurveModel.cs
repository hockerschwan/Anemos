namespace Anemos.Models;

public class LatchCurveModel : CurveModelBase
{
    private double _outputLowTemperature;
    public double OutputLowTemperature
    {
        get => _outputLowTemperature;
        set => SetProperty(ref _outputLowTemperature, value);
    }

    private double _outputHighTemperature;
    public double OutputHighTemperature
    {
        get => _outputHighTemperature;
        set => SetProperty(ref _outputHighTemperature, value);
    }

    private double _temperatureThresholdLow;
    public double TemperatureThresholdLow
    {
        get => _temperatureThresholdLow;
        set => SetProperty(ref _temperatureThresholdLow, value);
    }

    private double _temperatureThresholdHigh;
    public double TemperatureThresholdHigh
    {
        get => _temperatureThresholdHigh;
        set => SetProperty(ref _temperatureThresholdHigh, value);
    }

    private bool _useHigh = false;

    public LatchCurveModel(CurveArg args) : base(args)
    {
        _outputLowTemperature = args.OutputLowTemperature!.Value;
        _outputHighTemperature = args.OutputHighTemperature!.Value;
        _temperatureThresholdLow = args.TemperatureThresholdLow!.Value;
        _temperatureThresholdHigh = args.TemperatureThresholdHigh!.Value;
    }

    public override void Update()
    {
        Value = CalcValue();
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
