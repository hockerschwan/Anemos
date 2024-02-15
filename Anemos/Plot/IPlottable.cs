using Microsoft.Graphics.Canvas;

namespace Anemos.Plot;

public interface IPlottable
{
    void Draw(
        float width,
        float height,
        double xMin,
        double xMax,
        double yMin,
        double yMax,
        in CanvasDrawingSession session,
        in CanvasDevice device);

    public static float GetPixelX(double x, float width, double xMin, double xMax)
    {
        var u = width / (xMax - xMin);
        return (float)((x - xMin) * u);
    }

    public static float GetPixelY(double y, float height, double yMin, double yMax)
    {
        var u = height / (yMax - yMin);
        return (float)((yMax - y) * u);
    }
}
