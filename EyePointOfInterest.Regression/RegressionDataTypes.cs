using System.Numerics;
using Microsoft.ML.Data;
using OpenSense.Component.EyePointOfInterest.Common;
using OpenSense.Component.OpenFace.Common;

namespace OpenSense.Component.EyePointOfInterest.Regression {

    internal class RegressionRecord {
        [VectorType(8)]
        public float[] Cause = new float[8];

        [VectorType(2)]
        public float[] Result = new float[2];

        public float DisplayX {
            get => Result[0];
            set => Result[0] = value;
        }

        public float DisplayY {
            get => Result[1];
            set => Result[1] = value;
        }

        public RegressionRecord(GazeToDisplayCoordinateMappingRecord record) {
            Cause[0] = (float)record.Gaze.Angle.X;
            Cause[1] = (float)record.Gaze.Angle.Y;
            Cause[2] = (float)record.HeadPose.Position.X;
            Cause[3] = (float)record.HeadPose.Position.Y;
            Cause[4] = (float)record.HeadPose.Position.Z;
            Cause[5] = (float)record.HeadPose.Angle.X;
            Cause[6] = (float)record.HeadPose.Angle.Y;
            Cause[7] = (float)record.HeadPose.Angle.Z;

            DisplayX = (float)record.Display.X;
            DisplayY = (float)record.Display.Y;
        }

        public RegressionRecord(PoseAndEyeAndFace headPoseAndGaze) :this(new GazeToDisplayCoordinateMappingRecord(headPoseAndGaze, new Vector2())) {}
    }

    public class RegressionPrediction {
        [ColumnName("Score")]//Must be Score
        public float Value { get; set; }// must be Single
    }
}
