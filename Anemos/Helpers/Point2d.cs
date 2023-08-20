using System.Diagnostics;

namespace Anemos
{
    [DebuggerDisplay("{X}, {Y}")]
    public class Point2d
    {
        public double X
        {
            get;
        }

        public double Y
        {
            get;
        }

        public Point2d(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not Point2d other) { return false; }
            return other.X == X && other.Y == Y;
        }

        public override int GetHashCode() => System.HashCode.Combine(X, Y);
    }
}

namespace Anemos.Helpers
{
    public static class Point2dHelper
    {
        public static bool IsSorted(IEnumerable<Point2d> collection)
            => !collection.Zip(collection.Skip(1), (a, b) => a.X <= b.X).Contains(false);

        public static IEnumerable<Point2d> Sort(IEnumerable<Point2d> collection)
            => collection.ToList().OrderBy(e => e.X);
    }
}
