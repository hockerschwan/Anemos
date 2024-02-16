﻿using Anemos.Contracts.Services;

namespace Anemos.Models;

public enum CustomSensorCalcMethod
{
    Max, Min, Average, MovingAverage
}

public struct CustomSensorArg
{
    public CustomSensorCalcMethod CalcMethod;
    public string Id = string.Empty;
    public string Name = string.Empty;
    public int SampleSize = 1;
    public IEnumerable<string> SourceIds = Enumerable.Empty<string>();

    public CustomSensorArg()
    {
    }
}

public class CustomSensorModel : SensorModelBase
{
    private readonly ISensorService _sensorService = App.GetService<ISensorService>();

    public override string Name
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

    public override string LongName => Name;

    private List<string> _sourceIds = [];
    public List<string> SourceIds
    {
        get => _sourceIds;
        set
        {
            if (SetProperty(ref _sourceIds, value))
            {
                OnPropertyChanged(nameof(SourceModels));
                _sensorService.Save();
                Update();
            }
        }
    }

    public IEnumerable<SensorModelBase> SourceModels => _sensorService.GetSensors(SourceIds);

    private readonly LimitedQueue _data;

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
                _data.Capacity = SampleSize;
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
            if (value < 1) { return; }

            if (SetProperty(ref _sampleSize, value))
            {
                _sensorService.Save();
                _data.Capacity = _sampleSize;
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
        _data = new(args.SampleSize);
        _sourceIds = args.SourceIds.ToList();

        Update();
    }

    protected override void Update_()
    {
        Value = CalcValue();
    }

    private double? CalcValue()
    {
        double? value = null;
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
                if (_data.Count != 0)
                {
                    value = _data.Average();
                }
                break;
        }

        if (value == null)
        {
            return value;
        }
        return double.Round(value.Value, 1);
    }

    private static double? Average(in IEnumerable<SensorModelBase> sensors)
    {
        var sum = 0d;
        var count = 0;
        foreach (var s in sensors.ToList())
        {
            if (s.Value != null)
            {
                sum += s.Value.Value;
                ++count;
            }
        }

        return count == 0 ? null : sum / count;
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
