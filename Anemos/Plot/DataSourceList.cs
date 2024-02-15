namespace Anemos.Plot;

public readonly struct DataSourceList(List<double> xs, List<double> ys) : IScatterDataSource
{
    public readonly IList<double> GetXs() => xs;
    public readonly IList<double> GetYs() => ys;
}
