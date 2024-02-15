using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.UI;
using Windows.UI;

namespace Anemos.Plot;

public class Arrow : IPlottable
{
    private readonly DataSourceArray _source;
    public IList<double> GetXs() => _source.GetXs();
    public IList<double> GetYs() => _source.GetYs();

    public Color Color { get; set; } = Colors.CornflowerBlue;
    public float LineWidth { get; set; } = 2;
    public float HeadWidth { get; set; } = 10;
    public float HeadLength { get; set; } = 16;

    private readonly Vector2[] _headPoints = new Vector2[3];


    public Arrow(in double[] xs, in double[] ys)
    {
        if (xs.Length != 2 || ys.Length != 2)
        {
            throw new ArgumentException("Array length should be 2.", nameof(xs));
        }

        _source = new(xs, ys);
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
        var xs = _source.GetXs();
        var ys = _source.GetYs();

        var xBase = xs[0];
        var yBase = ys[0];
        var xTip = xs[1];
        var yTip = ys[1];

        var pxBase = IPlottable.GetPixelX(xBase, width, xMin, xMax);
        var pyBase = IPlottable.GetPixelY(yBase, height, yMin, yMax);
        var pxTip = IPlottable.GetPixelX(xTip, width, xMin, xMax);
        var pyTip = IPlottable.GetPixelY(yTip, height, yMin, yMax);

        var angle = Math.Atan2(pyTip - pyBase, pxTip - pxBase);
        if (LineWidth > 0)
        {
            var pBottom = Rotate(pxTip - HeadLength, pyTip, pxTip, pyTip, angle);
            session.DrawLine(pxBase, pyBase, pBottom.X, pBottom.Y, Color, LineWidth);
        }
        if (HeadWidth > 0 && HeadLength > 0)
        {
            _headPoints[0].X = pxTip;
            _headPoints[0].Y = pyTip;
            _headPoints[1] = Rotate(pxTip - HeadLength, pyTip + HeadWidth / 2, pxTip, pyTip, angle);
            _headPoints[2] = Rotate(pxTip - HeadLength, pyTip - HeadWidth / 2, pxTip, pyTip, angle);

            using var g = CanvasGeometry.CreatePolygon(session, _headPoints);

            session.DrawLine(pxTip, pyTip, pxBase, pyBase, Color, LineWidth);
            session.FillGeometry(g, Color);
        }
    }

    private static Vector2 Rotate(float x, float y, float xCenter, float yCenter, double angleRadians)
    {
        var sin = Math.Sin(angleRadians);
        var cos = Math.Cos(angleRadians);
        var dx = x - xCenter;
        var dy = y - yCenter;

        return new Vector2((float)(dx * cos - dy * sin + xCenter), (float)(dy * cos + dx * sin + yCenter));
    }
}
