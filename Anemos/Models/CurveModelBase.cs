using Anemos.Contracts.Services;
using Anemos.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum CurveType
{
    Chart, Latch
}

public class CurveArg
{
    public CurveType Type;
    public string Id = string.Empty;
    public string Name = string.Empty;
    public string SourceId = string.Empty;

    // Chart
    public IEnumerable<Point2>? Points;

    // Latch
    public double? OutputLowTemperature;
    public double? OutputHighTemperature;
    public double? TemperatureThresholdLow;
    public double? TemperatureThresholdHigh;
}

public class CurveModelBase : ObservableObject
{
    private protected readonly ICurveService _curveService = App.GetService<ICurveService>();
    private protected readonly ISensorService _sensorService = App.GetService<ISensorService>();

    public CurveType Type
    {
        get;
    }

    public string Id
    {
        get;
    }

    private protected string _name = string.Empty;
    public virtual string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                _curveService.Save();
            }
        }
    }

    private protected string _sourceId = string.Empty;
    public virtual string SourceId
    {
        get => _sourceId;
        set
        {
            if (SetProperty(ref _sourceId, value))
            {
                OnPropertyChanged(nameof(SourceModel));
                _curveService.Save();
                Update();
            }
        }
    }

    public SensorModelBase? SourceModel => _sensorService.GetSensor(SourceId);

    private protected double? _value;
    public virtual double? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public CurveModelBase(CurveArg args)
    {
        Type = args.Type;

        Id = args.Id == string.Empty ? Guid.NewGuid().ToString() : args.Id;

        _name = args.Name;

        if (args.SourceId != string.Empty)
        {
            _sourceId = args.SourceId;
        }
    }

    public virtual void Update()
    {
    }
}
