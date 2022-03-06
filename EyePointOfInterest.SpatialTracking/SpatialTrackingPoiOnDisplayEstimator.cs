using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using OpenSense.Component.EyePointOfInterest.Common;
using OpenSense.Component.Head.Common;
using static System.Math;

namespace OpenSense.Component.EyePointOfInterest.SpatialTracking {

    public class SpatialTrackingPoiOnDisplayEstimator : IPoiOnDisplayEstimator {

        public string Name { get; set; } = "Spatial Tracking";

        /// <summary>
        /// In millimeter
        /// </summary>
        private const double HeadDisplayDistance = 750;

        /// <summary>
        /// degree
        /// </summary>
        private const double FieldOfViewWidthSingleSideAngle = 30 / 360d * (2 * PI);
        private const double DisplayAspectRatio = 5d / 3d;
        private const double ReferenceDistance = HeadDisplayDistance;

        public int Order = 2;
        public GazeToDisplayCoordinateMappingRecord[] Samples = Array.Empty<GazeToDisplayCoordinateMappingRecord>();

        public SpatialTrackingPoiOnDisplayEstimator() { }

        public SpatialTrackingPoiOnDisplayEstimator(SpatialTrackingPoiOnDisplayEstimatorConfiguration data) {
            Order = data.Order;
            Samples = data.Samples;
        }

        private Point3D ConvertPointFromOpenFaceSpaceToMathNetSpace(Point3D openFacePoint) {
            //OpenFace: x positive left, y positive down, z positive near
            //Math.Net: x positive right, y positive far, z positive up
            return new Point3D(-openFacePoint.X, -openFacePoint.Z, -openFacePoint.Y);
        }

        private Point3D ConvertAngelFromOpenFaceSpaceToMathNetSpace(Point3D openFaceAngel) {
            //OpenFace: left-hand rule
            //Math.Net: positive right-hand rule (test)
            return new Point3D(openFaceAngel.X, openFaceAngel.Z, openFaceAngel.Y);
        }

        private Vector2D PupilCornerDistance(Eye gaze, Pose headPose) {
            var leftPupilCornerVector = gaze.PupilPosition.Left.ToMathNetPoint3D() - gaze.InnerEyeCornerPosition.Left.ToMathNetPoint3D();
            
            var headAngel = headPose.Angle;
            var headPitch = Angle.FromRadians(headAngel.X);
            var headYaw = Angle.FromRadians(headAngel.Y);
            var headRoll = Angle.FromRadians(headAngel.Z);
            var realigned = CoordinateSystem.Rotation(headYaw, headPitch, headRoll);
            leftPupilCornerVector = leftPupilCornerVector.TransformBy(realigned);
            
            var xDistance = leftPupilCornerVector.X;
            var yDistance = leftPupilCornerVector.Y;
            return new Vector2D(xDistance, yDistance);
        }

        private Point3D CalibPointOnDisplay(Point3D centroid, UnitVector3D norm, UnitVector3D xPositive, Vector2D size,  Point2D relativeCoordinate) {
            var zPositive = xPositive.CrossProduct(norm);
            return centroid + (relativeCoordinate.X - 0.5) * size.X * xPositive + ((1 - relativeCoordinate.Y) - 0.5) * size.Y * zPositive;
        }

        private Point2D RelativeToDisplay(Point3D centroid, UnitVector3D norm, UnitVector3D xPositive, Vector2D size, Point3D point) {
            var zPositive = xPositive.CrossProduct(norm);
            var vec = point - centroid;
            var x = vec.DotProduct(xPositive) / size.X + 0.5;
            var y = 1 - (vec.DotProduct(zPositive) / size.Y + 0.5);
            return new Point2D(x, y);
        }

        public Vector2 Train(IList<GazeToDisplayCoordinateMappingRecord> data) {
            Samples = data.ToArray();

            var predict = data.Select(r => Predict(new PoseAndEyeAndFace(r.HeadPose, r.Gaze, r.Face))).ToArray();
            var rSquaredX = GoodnessOfFit.RSquared(predict.Select(p => (double)p.X), data.Select(d => d.Display.X).Select(d => (double)d));
            var rSquaredY = GoodnessOfFit.RSquared(predict.Select(p => (double)p.Y), data.Select(d => d.Display.Y).Select(d => (double)d));
            return new Vector2((float)rSquaredX, (float)rSquaredY);
        }

        public Vector2 Predict(PoseAndEyeAndFace data) {

            var camera = new CoordinateSystem();
            var displayNormRealign = UnitVector3D.YAxis;
            var displayXPositiveRealign = UnitVector3D.XAxis;

            var refHeadPositionRaw = Point3D.Centroid(Samples.Select(r => r.HeadPose.Position).Select(r => r.ToMathNetPoint3D()));
            var refHeadPosition = ConvertPointFromOpenFaceSpaceToMathNetSpace(refHeadPositionRaw);
            var refHeadRotationRaw = Point3D.Centroid(Samples.Select(r => r.HeadPose.Angle).Select(r => r.ToMathNetPoint3D()));
            var refHeadRotation = ConvertAngelFromOpenFaceSpaceToMathNetSpace(refHeadRotationRaw);
            var refHeadRotationCoord = camera
                .RotateCoordSysAroundVector(UnitVector3D.XAxis, Angle.FromRadians(refHeadRotation.X))
                .RotateCoordSysAroundVector(UnitVector3D.ZAxis, Angle.FromRadians(refHeadRotation.Z))
                .RotateCoordSysAroundVector(UnitVector3D.YAxis, Angle.FromRadians(refHeadRotation.Y));
            var refHeadRotationAlign = CoordinateSystem.CreateMappingCoordinateSystem(camera, refHeadRotationCoord);
            var refHeadRotationUnalign = CoordinateSystem.CreateMappingCoordinateSystem(refHeadRotationCoord, camera);

            var displayWidth = HeadDisplayDistance * Tan(FieldOfViewWidthSingleSideAngle) * 2;
            var displayHeight = displayWidth / DisplayAspectRatio;
            var displaySize = new Vector2D(displayWidth, displayHeight);

            var refDisplayNorm = displayNormRealign.TransformBy(refHeadRotationUnalign).Normalize();
            var refDisplayXPositive = displayXPositiveRealign.TransformBy(refHeadRotationUnalign).Normalize();
            var refDisplayCentroid = refHeadPosition + HeadDisplayDistance * refDisplayNorm;
            var refDisplayPlane = new MathNet.Spatial.Euclidean.Plane(refDisplayCentroid, refDisplayNorm);

            var headPosition = ConvertPointFromOpenFaceSpaceToMathNetSpace(data.Pose.Position.ToMathNetPoint3D());
            var headRotation = ConvertAngelFromOpenFaceSpaceToMathNetSpace(data.Pose.Angle.ToMathNetPoint3D());
            var headRotationCoord = camera
                .RotateCoordSysAroundVector(UnitVector3D.XAxis, Angle.FromRadians(headRotation.X))
                .RotateCoordSysAroundVector(UnitVector3D.ZAxis, Angle.FromRadians(headRotation.Z))
                .RotateCoordSysAroundVector(UnitVector3D.YAxis, Angle.FromRadians(headRotation.Y));
            var headRotationAlign = CoordinateSystem.CreateMappingCoordinateSystem(camera, headRotationCoord);
            var headRotationUnalign = CoordinateSystem.CreateMappingCoordinateSystem(headRotationCoord, camera);

            var displayNorm = displayNormRealign.TransformBy(headRotationUnalign).Normalize();
            var displayXPositive = displayXPositiveRealign.TransformBy(headRotationUnalign).Normalize();
            var displayCentroid = headPosition + HeadDisplayDistance * displayNorm;
            var displayPlane = new MathNet.Spatial.Euclidean.Plane(displayCentroid, displayNorm);

            var calibPoints = Samples.Select(r => CalibPointOnDisplay(displayCentroid, displayNorm, displayXPositive, displaySize, r.Display.ToMathNetPoint2D())).ToArray();
            var calibRays = calibPoints.Select(p => new Ray3D(headPosition, p - headPosition)).ToArray();
            var transformedCalibPoints = calibRays.Select(r => (Point3D)r.IntersectionWith(refDisplayPlane)).ToArray();
            var transformedRelatives = transformedCalibPoints.Select(p => RelativeToDisplay(refDisplayCentroid, refDisplayNorm, refDisplayXPositive, displaySize, p)).ToArray();

            var pupilDistances = Samples.Select(r => PupilCornerDistance(r.Gaze, r.HeadPose)).ToArray();
            var inputX = pupilDistances.Select(d => d.X).ToArray();
            var outputX = transformedRelatives.Select(p => p.X).ToArray();
            var polynomialX = Polynomial.Fit(inputX, outputX, Order);
            var inputY = pupilDistances.Select(d => d.Y).ToArray();
            var outputY = transformedRelatives.Select(p => p.Y).ToArray();
            var polynomialY = Polynomial.Fit(inputY, outputY, Order);

            var distance = PupilCornerDistance(data.Eye, data.Pose);
            var x = polynomialX.Evaluate(distance.X);
            var y = polynomialY.Evaluate(distance.Y);
            return new Vector2((float)x, (float)y);
        }

        public PoiOnDisplayEstimatorConfiguration Save() {
            var config = new SpatialTrackingPoiOnDisplayEstimatorConfiguration() {
                Order = Order,
                Samples = Samples,
            };
            return config;
        }
    }
}
