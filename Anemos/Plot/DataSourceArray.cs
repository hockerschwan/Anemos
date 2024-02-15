namespace Anemos.Plot;

public readonly struct DataSourceArray(double[] xs, double[] ys) : IScatterDataSource
{
    public readonly IList<double> GetXs() => xs;
    public readonly IList<double> GetYs() => ys;
}
