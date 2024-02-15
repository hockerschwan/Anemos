using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Windows.UI;

namespace Anemos.Plot;

public class Scatter : IPlottable
{
    private readonly DataSourceArray? _sourceArray;
    private readonly DataSourceList? _sourceList;

    public IList<double> GetXs() => _sourceArray != null ? _sourceArray.Value.GetXs() : _sourceList!.Value.GetXs();
    public IList<double> GetYs() => _sourceArray != null ? _sourceArray.Value.GetYs() : _sourceList!.Value.GetYs();

    public Color Color { get; set; } = Colors.CornflowerBlue;
    public float LineWidth { get; set; } = 2;
    public float MarkerRadius { get; set; } = 0;


    public Scatter(in double[] xs, in double[] ys)
    {
        if (xs.Length != ys.Length)
        {
            throw new ArgumentException("Array length must be equal.", nameof(xs.Length));
        }

        _sourceArray = new(xs, ys);
    }

    public Scatter(in List<double> xs, in List<double> ys)
    {
        if (xs.Count != ys.Count)
        {
            throw new ArgumentException("List count must be equal.", nameof(xs.Count));
        }

        _sourceList = new(xs, ys);
    }

    public void Draw(
        float width,
        float height,
        double xMin,
        double xMax,
        double yMin,
        double yMax,
        in CanvasDrawingSession session,
        in CanvasDevice device)
    {
        var xs = GetXs();
        var ys = GetYs();

        var pxPrev = float.NaN;
        var pyPrev = float.NaN;

        for (var i = 0; i < xs.Count; ++i)
        {
            var x = xs[i];
            var y = ys[i];

            var px = IPlottable.GetPixelX(x, width, xMin, xMax);
            var py = IPlottable.GetPixelY(y, height, yMin, yMax);

            if (LineWidth > 0)
            {
                session.DrawLine(pxPrev, pyPrev, px, py, Color, LineWidth);
            }
            if (MarkerRadius > 0)
            {
                session.FillCircle(px, py, MarkerRadius, Color);
            }

            pxPrev = px;
            pyPrev = py;
        }
    }

    public Point2d[] GetScatterPoints()
    {
        var xs = GetXs();
        var ys = GetYs();

        var res = new Point2d[ys.Count];
        for (var i = 0; i < ys.Count; ++i)
        {
            res[i] = new Point2d(xs[i], ys[i]);
        }
        return res;
    }
}
