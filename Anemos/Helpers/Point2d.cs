using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Anemos
{
    [DebuggerDisplay("{X}, {Y}")]
    [method: JsonConstructor]
    public readonly struct Point2d(double x, double y)
    {
        public double X { get; } = x;

        public double Y { get; } = y;

        public static Point2d NaN => new(double.NaN, double.NaN);

        public readonly override bool Equals(object? obj)
        {
            if (obj is not Point2d other) { return false; }
            return Equals(other);
        }

        private readonly bool Equals(Point2d other)
        {
            if (double.IsNaN(X) && double.IsNaN(Y))
            {
                return double.IsNaN(other.X) && double.IsNaN(other.Y);
            }
            return other.X == X && other.Y == Y;
        }

        public readonly override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(Point2d left, Point2d right) => left.Equals(right);

        public static bool operator !=(Point2d left, Point2d right) => !(left == right);
    }
}

namespace Anemos.Helpers
{
    public static class Point2dHelper
    {
        public static bool IsSorted(in List<Point2d> collection)
        {
            if (collection.Count == 0) { return true; }

            var last = collection[0].X;
            for (var i = 1; i < collection.Count; ++i)
            {
                if (collection[i].X < last) { return false; }
                last = collection[i].X;
            }
            return true;
        }

        public static void Sort(ref List<Point2d> collection)
        {
            CollectionsMarshal.AsSpan(collection).Sort((a, b) =>
            {
                if (a.X < b.X) { return -1; }
                else if (a.X > b.X) { return 1; }
                return 0;
            });
        }
    }
}
