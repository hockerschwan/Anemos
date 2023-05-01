using Anemos.Helpers;

namespace Anemos.Models;

public class ChartCurveModel : CurveModelBase
{
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

    public ChartCurveModel(CurveArg args) : base(args)
    {
        _points = args.Points!.OrderBy(p => p.X);
    }

    public override void Update()
    {
        Value = CalcValue();
    }

    private double? CalcValue()
    {
        // assume points are sorted by X in ascending order

        if (SourceModel == null || SourceModel.Value == null || !Points.Any())
        {
            return null;
        }

        double? value;
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
            value = double.Round(Math.Max(0, Math.Min(100, value.Value)), 1);
        }
        return value;
    }
}
