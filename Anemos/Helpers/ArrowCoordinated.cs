using ScottPlot;
using ScottPlot.DataSources;
using ScottPlot.Extensions;
using SkiaSharp;

namespace Anemos.Helpers;

internal class ArrowCoordinated : IPlottable
{
    public bool IsVisible { get; set; } = true;

    public IAxes Axes { get; set; } = new Axes();

    public IEnumerable<LegendItem> LegendItems { get; } = Array.Empty<LegendItem>();

    public float ArrowheadLength { get; set; } = 20;

    public float ArrowheadWidth { get; set; } = 10;

    public bool IsHeadVisible { get; set; } = true;

    public LineStyle LineStyle { get; } = new();

    public MarkerStyle MarkerStyle
    {
        get;
    } = new(MarkerShape.FilledCircle, 5)
    {
        Outline = LineStyle.None,
        IsVisible = false
    };

    private readonly ScatterSourceCoordinates Source;

    public ArrowCoordinated(IReadOnlyList<Coordinates> coordinates)
    {
        if (coordinates.Count != 2)
        {
            throw new ArgumentException("Must contain exactly 2 cooordinates.", nameof(coordinates));
        }

        Source = new(coordinates);
    }

    public AxisLimits GetAxisLimits() => AxisLimits.NoLimits;

    public void Render(RenderPack rp)
    {
        var px_base = Axes.GetPixel(Source.GetScatterPoints()[0]);
        var px_tip = Axes.GetPixel(Source.GetScatterPoints()[1]);
        var length = Length(px_tip - px_base);
        if (length < 1f) { return; }

        var head_length = float.Clamp(length - 2, 0, ArrowheadLength);
        var angle = Math.Atan2(px_tip.Y - px_base.Y, px_tip.X - px_base.X);

        using SKPaint paint = new();

        // Line
        LineStyle.ApplyToPaint(paint);

        if (IsHeadVisible)
        {
            var px_tip_bottom = Rotate(px_tip.X - head_length, px_tip.Y, px_tip.X, px_tip.Y, angle);
            rp.Canvas.DrawLine(px_base.X, px_base.Y, px_tip_bottom.X, px_tip_bottom.Y, paint);
        }
        else
        {
            rp.Canvas.DrawLine(px_base.X, px_base.Y, px_tip.X, px_tip.Y, paint);
        }

        // Head
        if (IsHeadVisible && head_length >= 1)
        {
            var px_tip_top = new SKPoint(px_tip.X, px_tip.Y);

            using SKPath path = new();
            path.MoveTo(px_tip_top);
            path.LineTo(Rotate(px_tip.X - head_length, px_tip.Y + ArrowheadWidth / 2, px_tip.X, px_tip.Y, angle));
            path.LineTo(Rotate(px_tip.X - head_length, px_tip.Y - ArrowheadWidth / 2, px_tip.X, px_tip.Y, angle));
            path.LineTo(px_tip_top);

            paint.Style = SKPaintStyle.Fill;
            rp.Canvas.DrawPath(path, paint);
        }

        // Marker
        if (MarkerStyle.IsVisible)
        {
            MarkerStyle.Render(rp.Canvas, px_base);
        }
    }

    private static float Length(Pixel pixel) => float.Sqrt(float.Pow(pixel.X, 2) + float.Pow(pixel.Y, 2));

    private static SKPoint Rotate(float x, float y, float xCenter, float yCenter, double angleRadians)
    {
        var sin = Math.Sin(angleRadians);
        var cos = Math.Cos(angleRadians);

        var dx = x - xCenter;
        var dy = y - yCenter;

        return new SKPoint((float)(dx * cos - dy * sin + xCenter), (float)(dy * cos + dx * sin + yCenter));
    }
}
