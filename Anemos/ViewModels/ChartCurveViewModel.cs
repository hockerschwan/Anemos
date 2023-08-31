using System.ComponentModel;
using Anemos.Contracts.Services;
using Anemos.Models;

namespace Anemos.ViewModels;

internal class ChartCurveViewModel : CurveViewModelBase
{
    private ChartCurveModel CurveModel
    {
        get;
    }

    internal readonly List<double> LineDataX = new(16);
    internal readonly List<double> LineDataY = new(16);

    public ChartCurveViewModel(ChartCurveModel model) : base(model)
    {
        CurveModel = model;
        SetLineData();
    }

    internal override void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        base.Model_PropertyChanged(sender, e);

        if (e.PropertyName == nameof(CurveModel.Points))
        {
            SetLineData();
        }
    }

    private void SetLineData()
    {
        LineDataX.Clear();
        LineDataY.Clear();

        LineDataX.Add(ICurveService.AbsoluteMinTemperature);
        LineDataX.Add(ICurveService.AbsoluteMaxTemperature);

        if (CurveModel.Points.Any())
        {
            LineDataY.Add(CurveModel.Points.First().Y);
            LineDataY.Add(CurveModel.Points.Last().Y);
        }
        else
        {
            LineDataY.AddRange(Enumerable.Repeat(0d, 2));
        }

        LineDataX.InsertRange(1, CurveModel.Points.Select(p => p.X));
        LineDataY.InsertRange(1, CurveModel.Points.Select(p => p.Y));

        OnCurveDataChanged();
    }
}
