using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class ChartCurveEditorViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService = App.GetService<ISettingsService>();

    internal readonly List<double> LineDataX = new(16);
    internal readonly List<double> LineDataY = new(16);

    private int _selectedIndex = -1;
    internal int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (SetProperty(ref _selectedIndex, value) && _selectedIndex > 0)
            {
                var prev = FindPreviousX(LineDataX[value]);
                var next = FindNextX(LineDataX[value]);

                SelectedXMin = double.Max(_settingsService.Settings.CurveMinTemp, prev + 0.1);
                SelectedXMax = double.Min(_settingsService.Settings.CurveMaxTemp, next - 0.1);
            }
        }
    }

    private double _selectedX = double.NaN;
    internal double SelectedX
    {
        get => _selectedX;
        set
        {
            if (SelectedIndex < 0) { return; }

            if (double.IsNaN(value))
            {
                SetProperty(ref _selectedX, value);
                SelectedXMin = _settingsService.Settings.CurveMinTemp;
                SelectedXMax = _settingsService.Settings.CurveMaxTemp;
                return;
            }

            var prev = FindPreviousX(value);
            var next = FindNextX(value);

            var min = double.Max(_settingsService.Settings.CurveMinTemp, prev + 0.1);
            var max = double.Min(_settingsService.Settings.CurveMaxTemp, next - 0.1);

            value = double.Round(double.Clamp(value, min, max), 1);
            if (SetProperty(ref _selectedX, value))
            {
                LineDataX[SelectedIndex] = value;
            }
        }
    }

    private double _selectedY = double.NaN;
    internal double SelectedY
    {
        get => _selectedY;
        set
        {
            if (SelectedIndex < 0) { return; }

            if (double.IsNaN(value))
            {
                SetProperty(ref _selectedY, value);
                return;
            }

            value = double.Round(double.Clamp(value, 0, 100), 1);
            if (SetProperty(ref _selectedY, value))
            {
                LineDataY[SelectedIndex] = value;
            }
        }
    }

    private double _selectedXMin;
    public double SelectedXMin
    {
        get => _selectedXMin;
        set => SetProperty(ref _selectedXMin, value);
    }

    private double _selectedXMax;
    public double SelectedXMax
    {
        get => _selectedXMax;
        set => SetProperty(ref _selectedXMax, value);
    }

    public void SetPoints(ref IEnumerable<Point2d> points)
    {
        LineDataX.Clear();
        LineDataY.Clear();

        foreach (var point in points.ToList())
        {
            LineDataX.Add(point.X);
            LineDataY.Add(point.Y);
        }

        LineDataX.Insert(0, ICurveService.AbsoluteMinTemperature - 10d);
        LineDataX.Add(ICurveService.AbsoluteMaxTemperature + 10d);

        LineDataY.Insert(0, LineDataY.First());
        LineDataY.Add(LineDataY.Last());
    }

    public ChartCurveEditorViewModel()
    {
        _selectedXMin = _settingsService.Settings.CurveMinTemp;
        _selectedXMax = _settingsService.Settings.CurveMaxTemp;
    }

    private double FindNextX(double value)
    {
        foreach (var x in LineDataX)
        {
            if (x > value) { return x; };
        }
        return double.PositiveInfinity;
    }

    private double FindPreviousX(double value)
    {
        for (var i = LineDataX.Count - 1; i >= 0; --i)
        {
            if (LineDataX[i] < value) { return LineDataX[i]; };
        }
        return double.NegativeInfinity;
    }
}
