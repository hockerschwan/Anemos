using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ICurveService
{
    List<CurveModelBase> Curves
    {
        get;
    }

    Task InitializeAsync();

    void AddCurve(CurveArg arg);

    void RemoveCurve(string id);

    CurveModelBase? GetCurve(string id);

    void Save();
}
