namespace Anemos.Plot;

public readonly struct DataSourceSignal(IReadOnlyList<double> ys)
{
    public readonly IReadOnlyList<double> GetYs() => ys;
}
