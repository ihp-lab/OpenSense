using System.Numerics;
using MathNet.Spatial.Euclidean;

namespace OpenSense.Components.EyePointOfInterest.SpatialTracking {
    public static class SystemNumericsVectorExtensions {

        public static Point2D ToMathNetPoint2D(this Vector2 vec) => new Point2D(vec.X, vec.Y);

        public static Point3D ToMathNetPoint3D(this Vector3 vec) => new Point3D(vec.X, vec.Y, vec.Z);
    }
}
