using Anemos.Contracts.Services;
using Anemos.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public class CurveArg
{
    public string Id = string.Empty;
    public string Name = string.Empty;
    public string SourceId = string.Empty;
    public IEnumerable<Point2> Points = Enumerable.Empty<Point2>();
}

public class CurveModel : ObservableObject
{
    private readonly ICurveService _curveService;

    private readonly ISensorService _sensorService;

    private string _name;
    public string Name
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

    public string Id
    {
        get;
    }

    private string _sourceId = string.Empty;
    public string SourceId
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

    private IEnumerable<Point2> _points;
    public IEnumerable<Point2> Points
    {
        get => _points;
        set
        {
            if (SetProperty(ref _points, value))
            {
                _curveService.Save();
                Update();
            }
        }
    }

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public CurveModel(ICurveService curveService, ISensorService sensorService, CurveArg args)
    {
        _curveService = curveService;
        _sensorService = sensorService;

        _name = args.Name;

        Id = args.Id == string.Empty ? Guid.NewGuid().ToString() : args.Id;

        _points = args.Points.OrderBy(p => p.X);

        if (args.SourceId != string.Empty)
        {
            _sourceId = args.SourceId;
        }
    }

    public void Update()
    {
        Value = CalcValue();
    }

    private decimal? CalcValue()
    {
        // assume points are sorted by X in ascending order

        if (SourceModel == null || SourceModel.Value == null || !Points.Any())
        {
            return null;
        }

        decimal? value;
        var temperature = SourceModel.Value;
        var i = Points.ToList().FindIndex(p => p.X > temperature);
        switch (i)
        {
            case 0:
                // t < lowest
                value = Points.First().Y;
                break;
            case -1:
                // highest <= t
                value = Points.Last().Y;
                break;
            default:
                // else
                var lower = Points.ElementAt(i - 1);
                var higher = Points.ElementAt(i);
                value = lower.Y + (temperature - lower.X) * (higher.Y - lower.Y) / (higher.X - lower.X);
                break;
        }

        if (value != null)
        {
            value = decimal.Round(Math.Max(0m, Math.Min(100m, value.Value)), 1);
        }
        return value;
    }
}
