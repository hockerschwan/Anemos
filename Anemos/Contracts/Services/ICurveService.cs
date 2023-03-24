using Anemos.Models;

namespace Anemos.Contracts.Services;

public interface ICurveService
{
    List<CurveModel> Curves
    {
        get;
    }

    Task InitializeAsync();

    void AddCurve(CurveArg arg);

    void RemoveCurve(string id);

    CurveModel? GetCurve(string id);

    void Save();
}
