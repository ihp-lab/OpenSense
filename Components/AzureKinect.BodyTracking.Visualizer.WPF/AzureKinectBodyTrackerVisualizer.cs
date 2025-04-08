using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using MathNet.Spatial.Euclidean;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Imaging;
using OpenSense.WPF.Components.Psi.Imaging.Visualizer;
using Body = OpenSense.Components.AzureKinect.BodyTracking.Body;
using Image = Microsoft.Psi.Imaging.Image;

namespace OpenSense.WPF.Components.AzureKinect.BodyTracking.Visualizer {
    public sealed class AzureKinectBodyTrackerVisualizer : IConsumer<(Shared<Image>, Body[]?)>, IProducer<Shared<Image>>, INotifyPropertyChanged {

        private readonly ImageHolder _imageVisualizer = new();

        #region Options
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

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Ports
        public Receiver<IDepthDeviceCalibrationInfo> CalibrationIn { get; }

        public Receiver<(Shared<Image>, Body[]?)> In { get; }

        public Emitter<Shared<Image>> Out { get; }
        #endregion

        #region Binding Properties
        public WriteableBitmap Image => _imageVisualizer.Image;

        public double? FrameRate => _imageVisualizer.FrameRate;
        #endregion

        private IDepthDeviceCalibrationInfo? calibration;

        public AzureKinectBodyTrackerVisualizer(Pipeline pipeline) {
            CalibrationIn = pipeline.CreateReceiver<IDepthDeviceCalibrationInfo>(this, ProcessCalibration, nameof(CalibrationIn));
            In = pipeline.CreateReceiver<(Shared<Image>, Body[]?)>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            _imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, e);
            };
        }

        private void ProcessCalibration(IDepthDeviceCalibrationInfo calib, Envelope envelope) {
            calibration = calib;
        }

        private void Process((Shared<Image>, Body[]?) data, Envelope envelope) {
            if (Mute) {
                return;
            }
            if (calibration is null) {
                Logger?.LogInformation("Calibration is not available, discarding data.");
                return;
            }
            var (image, bodies) = data;
            if (bodies is null) {
                return;
            }
            using var bitmap = image.Resource.ToBitmap();
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
                foreach (var (parentJoin, childJoin) in Body.Bones) {
                    drawLine(parentJoin, childJoin);
                }
            }
            using var img = ImagePool.GetOrCreate(image.Resource.Width, image.Resource.Height, image.Resource.PixelFormat);
            img.Resource.CopyFrom(bitmap);
            _imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
            Out.Post(img, envelope.OriginatingTime);
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
