namespace Anemos.Models;

public class ChartCurveModel : CurveModelBase
{
    private IEnumerable<Point2d> _points;
    public IEnumerable<Point2d> Points
    {
        get => _points;
        set
        {
            if (!Helpers.Point2dHelper.IsSorted(value))
            {
                value = Helpers.Point2dHelper.Sort(value);
            }

            if (SetProperty(ref _points, value))
            {
                _curveService.Save();
                Update();
            }
        }
    }

    public ChartCurveModel(CurveArg args) : base(args)
    {
        _points = Helpers.Point2dHelper.Sort(args.Points!);
    }

    public override void Update()
    {
        Input = SourceModel?.Value;
        Output = CalcValue();
    }

    private double? CalcValue()
    {
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
