namespace Anemos.Models;

public class ChartCurveModel : CurveModelBase
{
    private List<Point2d> _points;
    public List<Point2d> Points
    {
        get => _points;
        set
        {
            if (!Helpers.Point2dHelper.IsSorted(value))
            {
                Helpers.Point2dHelper.Sort(ref value);
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
        _points = [.. args.Points!];
        Helpers.Point2dHelper.Sort(ref _points);
    }

    protected override void Update_()
    {
        Input = SourceModel?.Value;
        Output = CalcValue();
    }

    private double? CalcValue()
    {
        if (SourceModel == null || SourceModel.Value == null || Points.Count == 0)
        {
            return null;
        }

        double? value;
        var temperature = SourceModel.Value;
        var i = FindIndex(this, temperature);
        switch (i)
        {
            case 0:
                // t < lowest
                value = Points[0].Y;
                break;
            case -1:
                // highest <= t
                value = Points[^1].Y;
                break;
            default:
                // else
                var lower = Points[i - 1];
                var higher = Points[i];
                value = lower.Y + (temperature - lower.X) * (higher.Y - lower.Y) / (higher.X - lower.X);
                break;
        }

        if (value != null)
        {
            value = double.Round(double.Clamp(value.Value, 0, 100), 1);
        }
        return value;

        static int FindIndex(ChartCurveModel @this, double? temperature)
        {
            var index = 0;
            foreach (var point in @this.Points)
            {
                if (point.X > temperature) { return index; }
                ++index;
            }
            return -1;
        }
    }
}
