using System.Diagnostics;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Anemos.Models;

public enum CurveType
{
    Chart, Latch
}

[DebuggerDisplay("{Name}")]
public struct CurveArg
{
    public CurveType Type;
    public string Id = string.Empty;
    public string Name = string.Empty;
    public string SourceId = string.Empty;

    // Chart
    public List<Point2d>? Points;

    // Latch
    public double? OutputLowTemperature;
    public double? OutputHighTemperature;
    public double? TemperatureThresholdLow;
    public double? TemperatureThresholdHigh;

    public CurveArg()
    {
    }
}

[DebuggerDisplay("{Name}")]
public abstract class CurveModelBase : ObservableObject
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

    private protected double? _input;
    public virtual double? Input
    {
        get => _input;
        set => SetProperty(ref _input, value);
    }

    private protected double? _output;
    public virtual double? Output
    {
        get => _output;
        set => SetProperty(ref _output, value);
    }

    private readonly DispatcherQueueHandler _updateHandler;

    public CurveModelBase(CurveArg args)
    {
        Type = args.Type;

        Id = args.Id == string.Empty ? Guid.NewGuid().ToString() : args.Id;

        _name = args.Name;

        if (args.SourceId != string.Empty)
        {
            _sourceId = args.SourceId;
        }

        _updateHandler = Update_;
    }

    public void Update()
    {
        App.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High, _updateHandler);
    }

    protected abstract void Update_();
}
