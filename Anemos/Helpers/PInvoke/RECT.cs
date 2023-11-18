// http://www.pinvoke.net/default.aspx/Structures/RECT.html
using System.Runtime.InteropServices;

namespace Anemos.Helpers.PInvoke;

[StructLayout(LayoutKind.Sequential)]
public struct RECT(int left, int top, int right, int bottom)
{
    public int Left = left, Top = top, Right = right, Bottom = bottom;

    public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

    public int X
    {
        readonly get => Left;
        set
        {
            Right -= (Left - value);
            Left = value;
        }
    }

    public int Y
    {
        readonly get => Top;
        set
        {
            Bottom -= Top - value;
            Top = value;
        }
    }

    public int Height
    {
        readonly get => Bottom - Top;
        set => Bottom = value + Top;
    }

    public int Width
    {
        readonly get => Right - Left;
        set => Right = value + Left;
    }

    public System.Drawing.Point Location
    {
        readonly get => new(Left, Top);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

    public System.Drawing.Size Size
    {
        readonly get => new(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    public static implicit operator System.Drawing.Rectangle(RECT r) => new(r.Left, r.Top, r.Width, r.Height);

    public static implicit operator RECT(System.Drawing.Rectangle r) => new(r);

    public static bool operator ==(RECT r1, RECT r2) => r1.Equals(r2);

    public static bool operator !=(RECT r1, RECT r2) => !r1.Equals(r2);

    public readonly bool Equals(RECT r) => r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;

    public readonly override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is RECT rect)
        {
            return Equals(rect);
        }
        else if (obj is System.Drawing.Rectangle rectangle)
        {
            return Equals(new RECT(rectangle));
        }

        return false;
    }

    public readonly override int GetHashCode() => ((System.Drawing.Rectangle)this).GetHashCode();

    public readonly override string ToString()
        => $"{{Left={Left},Top={Top},Right={Right},Bottom={Bottom}}}";
}
