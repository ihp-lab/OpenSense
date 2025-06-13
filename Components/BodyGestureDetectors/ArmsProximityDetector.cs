using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Body = OpenSense.Components.AzureKinect.BodyTracking.Body;

namespace OpenSense.Components.BodyGestureDetectors {
    public sealed class ArmsProximityDetector : IConsumerProducer<IReadOnlyList<Body>?, double> {

        private const double DoubleFloatingPointTolerance = double.Epsilon * 2;

        #region Ports
        public Receiver<IReadOnlyList<Body>?> In { get; }

        /// <summary>
        /// Measured in meters.
        /// </summary>
        public Emitter<double> Out { get; }
        #endregion

        #region Settings
        private int bodyIndex = 0;

        public int BodyIndex {
            get => bodyIndex;
            set => SetProperty(ref bodyIndex, value);
        }

        private ConfidenceLevel minimumConfidenceLevel = ConfidenceLevel.Low;

        public ConfidenceLevel MinimumConfidenceLevel {
            get => minimumConfidenceLevel;
            set => SetProperty(ref minimumConfidenceLevel, value);
        }

        private double invalidValue = double.NaN;

        public double InvalidValue {
            get => invalidValue;
            set => SetProperty(ref invalidValue, value);
        }

        private bool postInvalidOnArmsNotDetected = true;

        public bool PostInvalidOnArmsNotDetected {
            get => postInvalidOnArmsNotDetected;
            set => SetProperty(ref postInvalidOnArmsNotDetected, value);
        }

        private bool postInvalidOnArmsNotOverlapped = true;

        public bool PostInvalidOnArmsNotOverlapped {
            get => postInvalidOnArmsNotOverlapped;
            set => SetProperty(ref postInvalidOnArmsNotOverlapped, value);
        }
        #endregion

        public ArmsProximityDetector(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IReadOnlyList<Body>?>(this, ProcessBodies, nameof(In));
            Out = pipeline.CreateEmitter<double>(this, nameof(Out));
        }

        private void ProcessBodies(IReadOnlyList<Body>? bodies, Envelope envelope) {
            if (bodies is null || bodies.Count <= bodyIndex) {
                return;
            }
            var body = bodies[bodyIndex];
            var leftWrist = body.Joints[JointId.WristLeft];
            var leftElbow = body.Joints[JointId.ElbowLeft];
            var rightWrist = body.Joints[JointId.WristRight];
            var rightElbow = body.Joints[JointId.ElbowRight];
            if (new[] { leftWrist, leftElbow, rightWrist, rightElbow }.Select(j => j.Confidence).Any(c => (int)c <= (int)MinimumConfidenceLevel)) {
                if (PostInvalidOnArmsNotDetected) {
                    Out.Post(InvalidValue, envelope.OriginatingTime);
                }
                return;
            }

            //Here we do not care about parallel
            var line1 = new Line3D(leftWrist.Pose.Origin, leftElbow.Pose.Origin);
            var line2 = new Line3D(rightWrist.Pose.Origin, rightElbow.Pose.Origin);
            var (pOnLine1, pOnLine2) = line1.ClosestPointsBetween(line2, mustBeOnSegments: false);
            var (pOnSeg1, pOnSeg2) = line1.ClosestPointsBetween(line2, mustBeOnSegments: true);
            var segProjOverlap = pOnLine1.Equals(pOnSeg1, tolerance: DoubleFloatingPointTolerance)
                && pOnLine2.Equals(pOnSeg2, tolerance: DoubleFloatingPointTolerance);
            if (!segProjOverlap) {
                if (PostInvalidOnArmsNotOverlapped) {
                    Out.Post(InvalidValue, envelope.OriginatingTime);
                }
                return;
            }
            var distance = pOnLine1.DistanceTo(pOnLine2);
            Out.Post(distance, envelope.OriginatingTime);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
