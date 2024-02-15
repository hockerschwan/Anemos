using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Windows.Foundation;
using Windows.UI;

namespace Anemos.Plot;

public class Plot
{
    public List<IPlottable> Plottables = new(4);

    public float AxisMargin { get; set; } = 16;

    public string LeftAxisLabel { get; set; } = string.Empty;
    public string BottomAxisLabel { get; set; } = string.Empty;

    public bool TopAxisIsVisible { get; set; } = true;
    public bool RightAxisIsVisible { get; set; } = true;
    public bool BottomAxisIsVisible { get; set; } = true;

    public bool LeftAxisTickIsVisible { get; set; } = true;
    public bool LeftAxisGridIsVisible { get; set; } = true;

    public bool BottomAxisTickIsVisible { get; set; } = true;
    public bool BottomAxisGridIsVisible { get; set; } = true;

    public double MinX { get; set; } = 0;
    public double MaxX { get; set; } = 10;

    public double MinY { get; set; } = 0;
    public double MaxY { get; set; } = 10;

    public double TickSpanX { get; set; } = 1;
    public double TickSpanY { get; set; } = 1;

    private readonly float _tickLength = 6;
    private readonly float _tickMargin = 4;
    private readonly float _labelMargin = 12;

    private readonly CanvasTextFormat _leftTickFormat = new()
    {
        FontSize = 12f,
        WordWrapping = CanvasWordWrapping.NoWrap,
        HorizontalAlignment = CanvasHorizontalAlignment.Right,
        VerticalAlignment = CanvasVerticalAlignment.Center
    };
    private readonly CanvasTextFormat _bottomTickFormat = new()
    {
        FontSize = 12f,
        WordWrapping = CanvasWordWrapping.NoWrap,
        HorizontalAlignment = CanvasHorizontalAlignment.Center,
        VerticalAlignment = CanvasVerticalAlignment.Top
    };
    private readonly CanvasTextFormat _labelFormat = new()
    {
        FontSize = 18f,
        WordWrapping = CanvasWordWrapping.NoWrap,
        HorizontalAlignment = CanvasHorizontalAlignment.Center
    };

    private readonly Matrix3x2 _translate0 = Matrix3x2.CreateRotation(0);
    private readonly Matrix3x2 _rotate270 = Matrix3x2.CreateRotation(-MathF.PI / 2);

    private readonly Color _bgColor = Color.FromArgb(255, 32, 32, 32);
    private readonly Color _borderColor = Color.FromArgb(255, 128, 128, 128);
    private readonly Color _gridColor = Color.FromArgb(255, 64, 64, 64);

    private Rect _plotRect;


    public void Draw(float width, float height, in CanvasDrawingSession session, in CanvasDevice device)
    {
        // bg
        session.Clear(_bgColor);


        float widthLeft;
        float leftTickLabelHalfHeight = 0;
        {
            var ex = (int)Math.Ceiling(Math.Log10(MaxY));
            var str = new string('0', ex);
            using var textLayout = new CanvasTextLayout(session, str, _leftTickFormat, 0f, 0f);
            leftTickLabelHalfHeight = float.Ceiling((float)textLayout.DrawBounds.Height / 2);
            widthLeft = AxisMargin + (float)textLayout.DrawBounds.Width + _tickMargin + _tickLength;

            // label
            if (LeftAxisLabel.Length > 0)
            {
                using var leftLabelLayout = new CanvasTextLayout(session, LeftAxisLabel, _labelFormat, 0f, 0f);
                session.Transform = _rotate270;
                session.DrawTextLayout(
                    leftLabelLayout,
                    (float)leftLabelLayout.DrawBounds.Height + _labelMargin - height / 2,
                    AxisMargin - (float)leftLabelLayout.DrawBounds.Top,
                    Colors.White);
                session.Transform = _translate0;

                widthLeft += (float)leftLabelLayout.DrawBounds.Height + _labelMargin;
            }
        }

        var heightBottom = AxisMargin;
        var bottomTickLabelHalfWidth = 0f;
        if (BottomAxisIsVisible)
        {
            using var textLayout = new CanvasTextLayout(session, MaxX.ToString(), _bottomTickFormat, 0f, 0f);
            bottomTickLabelHalfWidth = float.Ceiling((float)textLayout.DrawBounds.Width / 2);
            heightBottom = AxisMargin + (float)textLayout.DrawBounds.Height + _tickMargin + _tickLength;

            // label
            if (BottomAxisLabel.Length > 0)
            {
                using var bottomLabelLayout = new CanvasTextLayout(session, BottomAxisLabel, _labelFormat, 0f, 0f);
                session.DrawTextLayout(
                    bottomLabelLayout,
                    width / 2 - (float)bottomLabelLayout.DrawBounds.Width / 2 + widthLeft - bottomTickLabelHalfWidth,
                    height - AxisMargin - (float)(bottomLabelLayout.DrawBounds.Height + textLayout.DrawBounds.Top),
                    Colors.White);

                heightBottom += (float)bottomLabelLayout.DrawBounds.Height + _labelMargin;
            }
        }

        var heightTop = AxisMargin;
        if (TopAxisIsVisible && leftTickLabelHalfHeight >= AxisMargin)
        {
            heightTop = leftTickLabelHalfHeight + AxisMargin / 2;
        }

        var widthRight = AxisMargin;
        if (RightAxisIsVisible && BottomAxisIsVisible && BottomAxisTickIsVisible && bottomTickLabelHalfWidth >= AxisMargin)
        {
            widthRight = bottomTickLabelHalfWidth + AxisMargin / 2;
        }

        if (width <= widthLeft + widthRight || height <= heightTop + heightBottom) { return; }
        _plotRect = new Rect(widthLeft, heightTop, width - widthLeft - widthRight, height - heightTop - heightBottom);
        var left = (float)_plotRect.Left;
        var top = (float)_plotRect.Top;
        var right = (float)_plotRect.Right;
        var bottom = (float)_plotRect.Bottom;


        // tick and grid
        {
            if (LeftAxisTickIsVisible)
            {
                double tickSpan;
                {
                    using var textLayout = new CanvasTextLayout(session, "0123456789", _leftTickFormat, 0f, 0f);
                    var m = (MaxY - MinY) / Math.Floor(height / (2.5 * textLayout.DrawBounds.Height));
                    var ex = (int)Math.Floor(Math.Log10(m));
                    m /= Math.Pow(10, ex);
                    if (m <= 1)
                    {
                        m = 1;
                    }
                    else if (m <= 2)
                    {
                        m = 2;
                    }
                    else if (m <= 2.5)
                    {
                        m = 2.5;
                    }
                    else if (m <= 5)
                    {
                        m = 5;
                    }
                    else
                    {
                        m = 10;
                    }
                    tickSpan = m * Math.Pow(10, ex);
                }

                var i = (int)double.Floor(MaxY / tickSpan);
                if (!TopAxisIsVisible)
                {
                    i += 1;
                }

                var j = (int)double.Floor(MinY / tickSpan);
                if (!BottomAxisIsVisible)
                {
                    j -= 1;
                }

                for (; i >= j; --i)
                {
                    var y = tickSpan * i;
                    var py = GetPixelY(y);
                    if (LeftAxisGridIsVisible)
                    {
                        session.DrawLine(left, py, right, py, _gridColor);
                    }
                    session.DrawLine(left - _tickLength, py, left, py, _borderColor);
                    session.DrawText(
                        y.ToString(),
                        left - _tickLength - _tickMargin,
                        py,
                        Colors.White,
                        _leftTickFormat);
                }
            }

            if (BottomAxisIsVisible && BottomAxisTickIsVisible)
            {
                double tickSpan;
                {
                    var strMin = MinX.ToString();
                    var strMax = MaxX.ToString();
                    using var textLayout = new CanvasTextLayout(
                        session,
                        strMin.Length > strMax.Length ? strMin : strMax,
                        _bottomTickFormat,
                        0f,
                        0f);
                    var m = (MaxX - MinX) / Math.Floor(height / (1.5 * textLayout.DrawBounds.Width));
                    var ex = (int)Math.Floor(Math.Log10(m));
                    m /= Math.Pow(10, ex);
                    if (m <= 1)
                    {
                        m = 1;
                    }
                    else if (m <= 2)
                    {
                        m = 2;
                    }
                    else if (m <= 2.5)
                    {
                        m = 2.5;
                    }
                    else if (m <= 5)
                    {
                        m = 5;
                    }
                    else
                    {
                        m = 10;
                    }
                    tickSpan = m * Math.Pow(10, ex);
                }

                var x = double.Floor(MinX / tickSpan) * tickSpan;
                for (; x <= MaxX; x += tickSpan)
                {
                    var px = GetPixelX(x);
                    if (BottomAxisGridIsVisible)
                    {
                        session.DrawLine(px, top, px, bottom, _gridColor);
                    }
                    session.DrawLine(px, bottom, px, bottom + _tickLength, _borderColor);
                    session.DrawText(
                        x.ToString(),
                        px,
                        bottom + _tickLength + _tickMargin,
                        Colors.White,
                        _bottomTickFormat);
                }
            }
        }


        // border
        {
            session.DrawLine(
                left,
                TopAxisIsVisible ? top : 0f,
                left,
                BottomAxisIsVisible ? bottom : height,
                _borderColor);

            if (BottomAxisIsVisible)
            {
                session.DrawLine(left, bottom, right, bottom, _borderColor);
            }

            if (TopAxisIsVisible)
            {
                session.DrawLine(left, top, right, top, _borderColor);
            }

            if (RightAxisIsVisible)
            {
                session.DrawLine(right, top, right, bottom, _borderColor);
            }
        }


        // plottables
        var clipRect = new Rect(left - 1, top - 1, right - left + 2, bottom - top + 2);
        using (session.CreateLayer(1, clipRect))
        {
            session.Transform = Matrix3x2.CreateTranslation(left, top);
            foreach (var pl in Plottables)
            {
                pl.Draw((float)_plotRect.Width, (float)_plotRect.Height, MinX, MaxX, MinY, MaxY, session, device);
            }
            session.Transform = _translate0;
        }
    }

    public float GetPixelX(double x)
    {
        var u = _plotRect.Width / (MaxX - MinX);
        return float.Round((float)(_plotRect.Left + (x - MinX) * u), 0);
    }

    public float GetPixelY(double y)
    {
        var u = _plotRect.Height / (MaxY - MinY);
        return float.Round((float)(_plotRect.Top - (y - MaxY) * u), 0);
    }

    public double GetPointX(float x)
    {
        var u = (MaxX - MinX) / _plotRect.Width;
        return MinX + (x - _plotRect.Left) * u;
    }

    public double GetPointY(float y)
    {
        var u = (MaxY - MinY) / _plotRect.Height;
        return MaxY - (y - _plotRect.Top) * u;
    }
}
