using Microsoft.Graphics.Canvas;
using Microsoft.UI;
using Windows.UI;

namespace Anemos.Plot;

public class Signal(in IReadOnlyList<double> ys) : IPlottable
{
    private readonly DataSourceSignal _source = new(ys);
    public IReadOnlyList<double> GetYs() => _source.GetYs();

    public Color Color { get; set; } = Colors.CornflowerBlue;
    public float LineWidth { get; set; } = 2;

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
        var ys = _source.GetYs();

        var pxPrev = IPlottable.GetPixelX(0, width, xMin, xMax);
        var pyPrev = IPlottable.GetPixelY(ys[0], height, yMin, yMax);

        for (var i = 1; i < ys.Count; ++i)
        {
            var y = ys[i];
            var i0 = i;
            while (i < ys.Count - 1 && y == ys[i + 1])
            {
                ++i;
            }

            if (i == i0)
            {
                var px = IPlottable.GetPixelX(i, width, xMin, xMax);
                var py = IPlottable.GetPixelY(y, height, yMin, yMax);

                session.DrawLine(pxPrev, pyPrev, px, py, Color, LineWidth);

                pxPrev = px;
                pyPrev = py;
            }
            else
            {
                var px = IPlottable.GetPixelX(i0, width, xMin, xMax);
                var py = IPlottable.GetPixelY(y, height, yMin, yMax);
                session.DrawLine(pxPrev, pyPrev, px, py, Color, LineWidth);

                pxPrev = px;
                pyPrev = py;
                px = IPlottable.GetPixelX(i, width, xMin, xMax);
                session.DrawLine(pxPrev, pyPrev, px, pyPrev, Color, LineWidth);

                pxPrev = px;
            }
        }
    }
}
