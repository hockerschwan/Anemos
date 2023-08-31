using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ICurveService
{
    const double AbsoluteMinTemperature = -273d;
    const double AbsoluteMaxTemperature = 150d;

    List<CurveModelBase> Curves
    {
        get;
    }

    void AddCurve(CurveArg arg);

    CurveModelBase? GetCurve(string id);

    Task LoadAsync();

    void RemoveCurve(string id);

    void Save();
}
