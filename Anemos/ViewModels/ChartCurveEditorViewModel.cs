using Anemos.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Anemos.ViewModels;

public class ChartCurveEditorViewModel : ObservableObject
{
    internal readonly List<double> LineDataX = new(16);
    internal readonly List<double> LineDataY = new(16);

    internal double MinTemperature => ICurveService.AbsoluteMinTemperature;
    internal double MaxTemperature => ICurveService.AbsoluteMaxTemperature;

    private int _selectedIndex = -1;
    internal int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    private double _selectedX = double.NaN;
    internal double SelectedX
    {
        get => _selectedX;
        set => SetProperty(ref _selectedX, value);
    }

    private double _selectedY = double.NaN;
    internal double SelectedY
    {
        get => _selectedY;
        set => SetProperty(ref _selectedY, value);
    }

    public void SetPoints(ref IEnumerable<Point2d> points)
    {
        LineDataX.Clear();
        LineDataY.Clear();

        foreach (var point in points)
        {
            LineDataX.Add(point.X);
            LineDataY.Add(point.Y);
        }

        LineDataX.Insert(0, ICurveService.AbsoluteMinTemperature - 10d);
        LineDataX.Add(ICurveService.AbsoluteMaxTemperature + 10d);

        LineDataY.Insert(0, LineDataY.First());
        LineDataY.Add(LineDataY.Last());
    }
}
