﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.WPF.Components.Psi.Imaging.Visualizer;
using Image = Microsoft.Psi.Imaging.Image;

namespace OpenSense.Components.AzureKinect.Visualizer {
    public class AzureKinectBodyTrackerVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

        private Connector<List<AzureKinectBody>> BodiesInConnector;

        private Connector<IDepthDeviceCalibrationInfo> CalibrationInConnector;

        private Connector<Shared<Image>> ColorImageInConnector;

        #region Ports
        public Receiver<List<AzureKinectBody>> BodiesIn => BodiesInConnector.In;

        public Receiver<IDepthDeviceCalibrationInfo> CalibrationIn => CalibrationInConnector.In;

        public Receiver<Shared<Image>> ColorImageIn => ColorImageInConnector.In;

        public Emitter<Shared<Image>> Out { get; private set; }
        #endregion

        #region Settings
        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private int circleRadius = 3;

        public int CircleRadius {
            get => circleRadius;
            set => SetProperty(ref circleRadius, value);
        }

        private int lineThickness = 1;

        public int LineThickness {
            get => lineThickness;
            set => SetProperty(ref lineThickness, value);
        } 
        #endregion

        #region Binding Properties
        public WriteableBitmap Image => imageVisualizer.Image;

        public double? FrameRate => imageVisualizer.FrameRate;
        #endregion

        private ImageHolder imageVisualizer = new ImageHolder();

        public AzureKinectBodyTrackerVisualizer(Pipeline pipeline) : base(pipeline) {
            BodiesInConnector = CreateInputConnectorFrom<List<AzureKinectBody>>(pipeline, nameof(BodiesIn));
            CalibrationInConnector = CreateInputConnectorFrom<IDepthDeviceCalibrationInfo>(pipeline, nameof(CalibrationIn));
            ColorImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ColorImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined1 = BodiesInConnector.Out.Fuse(CalibrationInConnector.Out, Available.Nearest<IDepthDeviceCalibrationInfo>());//Note: Calibration only given once, Join is not aplicable here
            var joined2 = joined1.Join(ColorImageInConnector.Out, Reproducible.Nearest<Shared<Image>>());
            joined2.Do(Process);

            imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        public void RenderingCallback(object sender, EventArgs args) => imageVisualizer.RenderingCallback(sender, args);

        private void Process(ValueTuple<List<AzureKinectBody>, IDepthDeviceCalibrationInfo, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            var (bodies, calibration, frame) = data;
            lock (this) {
                //draw
                if (frame?.Resource != null) {
                    var bitmap = frame.Resource.ToBitmap();
                    using var linePen = new Pen(Color.LightGreen, LineThickness);
                    using var circleBrush = new SolidBrush(Color.LightGreen);
                    using var graphics = Graphics.FromImage(bitmap);
                    foreach (var body in bodies) {
                        void drawLine(JointId joint1, JointId joint2) {
                            var p1_3d = body.Joints[joint1].Pose.Origin;
                            var p2_3d = body.Joints[joint2].Pose.Origin;
                            var p1nullable = calibration.GetPixelPosition(p1_3d);
                            var p2nullable = calibration.GetPixelPosition(p2_3d);
                            if (p1nullable is Point2D p1 && p2nullable is Point2D p2 && IsValidPoint2D(p1) && IsValidPoint2D(p2)) {
                                var _p1 = new PointF((float)p1.X, (float)p1.Y);
                                var _p2 = new PointF((float)p2.X, (float)p2.Y);
                                graphics.DrawLine(linePen, _p1, _p2);
                                graphics.FillEllipse(circleBrush, _p1.X, _p1.Y, circleRadius, circleRadius);
                                graphics.FillEllipse(circleBrush, _p2.X, _p2.Y, circleRadius, circleRadius);
                            }
                        }
                        foreach (var (parentJoin, childJoin) in AzureKinectBody.Bones) {
                            drawLine(parentJoin, childJoin);
                        }
                    }
                    using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
                    img.Resource.CopyFrom(bitmap);
                    imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
                    Out.Post(img, envelope.OriginatingTime);
                }
            }
        }

        #region Helpers
        private static bool IsValidDouble(double val) {
            if (double.IsNaN(val)) {
                return false;
            }
            if (double.IsInfinity(val)) {
                return false;
            }
            return true;
        }

        private static bool IsValidPoint2D(Point2D point) => IsValidDouble(point.X) && IsValidDouble(point.Y);
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
