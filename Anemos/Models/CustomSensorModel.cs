using Anemos.Contracts.Models;
using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.Models;

public enum CustomSensorCalcMethod
{
    Max, Min, Average, MovingAverage
}

public class CustomSensorArg
{
    public CustomSensorCalcMethod CalcMethod;
    public string Id = string.Empty;
    public string Name = string.Empty;
    public int SampleSize = 1;
    public IEnumerable<string> SourceIds = Enumerable.Empty<string>();
}

public class CustomSensorModel : ObservableObject, ISensorModel
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    private readonly string _id;
    public string Id => _id;

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                OnPropertyChanged(nameof(LongName));
                _sensorService.Save();
            }
        }
    }

    public string LongName => Name;

    private decimal? _value;
    public decimal? Value
    {
        get => _value;
        private set => SetProperty(ref _value, value);
    }

    private List<string> _sourceIds = new();
    public List<string> SourceIds
    {
        get => _sourceIds;
        private set
        {
            if (SetProperty(ref _sourceIds, value))
            {
                OnPropertyChanged(nameof(SourceModels));
                _sensorService.Save();
                Update();
            }
        }
    }

    public IEnumerable<ISensorModel> SourceModels => _sensorService.GetSensors(SourceIds);

    private LimitedQueue<decimal> _data = new(1);

    private CustomSensorCalcMethod _calcMethod;
    public CustomSensorCalcMethod CalcMethod
    {
        get => _calcMethod;
        set
        {
            if (SetProperty(ref _calcMethod, value))
            {
                _sensorService.Save();
            }

            if (value == CustomSensorCalcMethod.MovingAverage)
            {
                _data = new(SampleSize);
            }

            Update();
        }
    }

    private int _sampleSize;
    public int SampleSize
    {
        get => _sampleSize;
        set
        {
            if (value < 0) { return; }

            if (SetProperty(ref _sampleSize, value))
            {
                _sensorService.Save();
                _data.Limit = _sampleSize;
                Update();
            }
        }
    }

    public CustomSensorModel(CustomSensorArg args)
    {
        _id = args.Id == string.Empty ? Guid.NewGuid().ToString() : args.Id;
        _name = args.Name;
        _calcMethod = args.CalcMethod;
        _sampleSize = args.SampleSize;
        _sourceIds = args.SourceIds.ToList();

        Update();
    }

    public void Update()
    {
        Value = CalcValue();
    }

    private decimal? CalcValue()
    {
        decimal? value = null;
        if (!SourceModels.Any())
        {
            return value;
        }

        switch (CalcMethod)
        {
            case CustomSensorCalcMethod.Max:
                var max = SourceModels.Select(s => s.Value).Max();
                if (max != null)
                {
                    value = max.Value;
                }
                break;
            case CustomSensorCalcMethod.Min:
                var min = SourceModels.Select(s => s.Value).Min();
                if (min != null)
                {
                    value = min.Value;
                }
                break;
            case CustomSensorCalcMethod.Average:
                value = Average(SourceModels);
                break;
            case CustomSensorCalcMethod.MovingAverage:
                if (!SourceModels.Any()) { break; }

                var average = Average(SourceModels);
                if (average != null)
                {
                    _data.Enqueue(average.Value);
                }
                if (_data.Count > 0)
                {
                    value = _data.Sum() / _data.Count;
                }
                break;
        }

        if (value == null)
        {
            return value;
        }
        return decimal.Round(value.Value, 1);
    }

    private static decimal? Average(IEnumerable<ISensorModel> sensors)
    {
        var validSensors = sensors.Where(s => s.Value != null).ToList();
        if (validSensors.Any())
        {
            return validSensors.Sum(s => s.Value)! / validSensors.Count;
        }
        return null;
    }

    public void AddSource(string id)
    {
        if (SourceIds.Contains(id)) { return; }

        SourceIds.Add(id);
        OnPropertyChanged(nameof(SourceModels));
        _sensorService.Save();
        Update();
    }

    public void RemoveSource(string id)
    {
        SourceIds.Remove(id);
        OnPropertyChanged(nameof(SourceModels));
        _sensorService.Save();
        Update();
    }

}
