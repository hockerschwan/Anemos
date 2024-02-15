namespace Anemos.Plot;

public interface IScatterDataSource
{
    IList<double> GetXs();
    IList<double> GetYs();
}
