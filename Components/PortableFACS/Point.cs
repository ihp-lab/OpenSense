using System.Collections.Generic;

namespace OpenSense.Components.PortableFACS {
    internal record struct Point(float X, float Y) {

        public static implicit operator Point((float, float) p) => new Point(p.Item1, p.Item2);
        public static implicit operator (float, float)(Point p) => (p.X, p.Y);
        public static Point operator +(Point left, Point right) => new Point(left.X + right.X, left.Y + right.Y);
        public static Point operator -(Point left, Point right) => new Point(left.X - right.X, left.Y - right.Y);
        public static Point operator *(Point left, float right) => new Point(left.X * right, left.Y * right);

        public static Point Average(IEnumerable<Point> points) {
            var count = 0;
            var x = 0d;
            var y = 0d;
            foreach (var point in points) {
                x += point.X;
                y += point.Y;
                count++;
            }
            var result = new Point((float)(x / count), (float)(y / count));
            return result;
        }
    }
}
