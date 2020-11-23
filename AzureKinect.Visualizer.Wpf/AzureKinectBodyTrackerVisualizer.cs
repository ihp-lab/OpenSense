using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;
using Microsoft.Psi.Calibration;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenSense.Component.Imaging.Visualizer.Common;

namespace OpenSense.Component.AzureKinect.Visualizer {
    public class AzureKinectBodyTrackerVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Connector<List<AzureKinectBody>> BodiesInConnector;

        private Connector<IDepthDeviceCalibrationInfo> CalibrationInConnector;

        private Connector<Shared<Image>> ColorImageInConnector;

        public Receiver<List<AzureKinectBody>> BodiesIn => BodiesInConnector.In;

        public Receiver<IDepthDeviceCalibrationInfo> CalibrationIn => CalibrationInConnector.In;

        public Receiver<Shared<Image>> ColorImageIn => ColorImageInConnector.In;

        public Emitter<Shared<Image>> Out { get; private set; }

        private DisplayVideo display = new DisplayVideo();

        public WriteableBitmap Image {
            get => display.VideoImage;
        }

        public int FrameRate {
            get => display.ReceivedFrames.Rate;
        }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private int jointRadius = 3;

        public int JointRadius {
            get => jointRadius;
            set => SetProperty(ref jointRadius, value);
        }

        private int boneThickness = 1;

        public int BoneThickness {
            get => boneThickness;
            set => SetProperty(ref boneThickness, value);
        }

        public AzureKinectBodyTrackerVisualizer(Pipeline pipeline) : base(pipeline) {
            BodiesInConnector = CreateInputConnectorFrom<List<AzureKinectBody>>(pipeline, nameof(BodiesIn));
            CalibrationInConnector = CreateInputConnectorFrom<IDepthDeviceCalibrationInfo>(pipeline, nameof(CalibrationIn));
            ColorImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ColorImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined1 = BodiesInConnector.Out.Fuse(CalibrationInConnector.Out, Available.Nearest<IDepthDeviceCalibrationInfo>());//Note: Calibration only given once, Join is not aplicable here
            var joined2 = joined1.Join(ColorImageInConnector.Out, Reproducible.Nearest<Shared<Image>>());
            joined2.Do(Process);

            pipeline.PipelineCompleted += OnPipelineCompleted;

            display.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.VideoImage)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));
                }
            };
            display.ReceivedFrames.PropertyChanged += (sender, e) => {
                if (e.PropertyName == nameof(display.RenderedFrames.Rate)) {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FrameRate)));
                }
            };
        }

        private void Process(ValueTuple<List<AzureKinectBody>, IDepthDeviceCalibrationInfo, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            var (bodies, calibration, frame) = data;
            lock (this) {
                //draw
                if (frame != null && frame.Resource != null) {
                    var bitmap = frame.Resource.ToBitmap();
                    try {//native opencv
                        var mat = bitmap.ToMat();//data copied
                        foreach (var body in bodies) {
                            void drawLine(JointId joint1, JointId joint2) {
                                var p1_3d = body.Joints[joint1].Pose.Origin;
                                var p2_3d = body.Joints[joint2].Pose.Origin;
                                var p1 = calibration.ToColorSpace(p1_3d);
                                var p2 = calibration.ToColorSpace(p2_3d);

                                if ((p1.X != 0 || p1.Y != 0) && (p2.X != 0 || p2.Y != 0)) {
                                    var _p1 = new Point(p1.X, p1.Y);
                                    var _p2 = new Point(p2.X, p2.Y);
                                    mat.Line(_p1, _p2, Scalar.LightGreen, BoneThickness, lineType: LineTypes.Link8);
                                    mat.Circle(_p1, JointRadius, Scalar.LightGreen, thickness: -1, lineType: LineTypes.Link8);
                                    mat.Circle(_p2, JointRadius, Scalar.LightGreen, thickness: -1, lineType: LineTypes.Link8);
                                }
                            }
                            foreach (var bone in AzureKinectBody.Bones) {
                                drawLine(bone.ParentJoint, bone.ChildJoint);
                            }
                        }
                        mat.ToBitmap(bitmap);
                    } catch (Exception ex) {
                        Mute = true;
                        throw;
                    }
                    using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
                    img.Resource.CopyFrom(bitmap);
                    Out.Post(img, envelope.OriginatingTime);
                    display.Update(img);
                }
            }
        }

        private void OnPipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }
    }
}
