using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.Components.Imaging.Visualizer;
using Image = Microsoft.Psi.Imaging.Image;

namespace OpenSense.Components.OpenFace.Visualizer {
    public sealed class OpenFaceVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Ports
        private Connector<PoseAndEyeAndFace> DataInConnector;

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<PoseAndEyeAndFace> DataIn => DataInConnector.In;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

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

        private bool drawHeadLandmarks = true;

        public bool DrawHeadLandmarks {
            get => drawHeadLandmarks;
            set => SetProperty(ref drawHeadLandmarks, value);
        }

        private bool drawHeadIndicatorLines = true;

        public bool DrawHeadIndicatorLines {
            get => drawHeadIndicatorLines;
            set => SetProperty(ref drawHeadIndicatorLines, value);
        }

        private bool drawEyeLandmarks = true;

        public bool DrawEyeLandmarks {
            get => drawEyeLandmarks;
            set => SetProperty(ref drawEyeLandmarks, value);
        }

        private bool drawEyeIndicatorLines = true;

        public bool DrawEyeIndicatorLines {
            get => drawEyeIndicatorLines;
            set => SetProperty(ref drawEyeIndicatorLines, value);
        }
        #endregion

        #region Binding Properties
        public WriteableBitmap Image => imageVisualizer.Image;

        public double? FrameRate => imageVisualizer.FrameRate;
        #endregion

        private ImageVisualizer imageVisualizer = new ImageVisualizer();

        public OpenFaceVisualizer(Pipeline pipeline) : base(pipeline) {
            DataInConnector = CreateInputConnectorFrom<PoseAndEyeAndFace>(pipeline, nameof(DataIn));
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined = DataInConnector.Out.Join(ImageInConnector.Out, Reproducible.Exact<Shared<Image>>());
            joined.Do(Process);

            imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        private void Process(ValueTuple<PoseAndEyeAndFace, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            var (datum, frame) = data;
            if (frame?.Resource != null) {
                var bitmap = frame.Resource.ToBitmap();
                using var linePen = new Pen(Color.LightGreen, LineThickness);
                using var circleBrush = new SolidBrush(Color.LightGreen);
                using var graphics = Graphics.FromImage(bitmap);
                void drawLine(PointF p1, PointF p2) {
                    if ((p1.X == 0 && p1.Y == 0) || (p2.X == 0 && p2.Y == 0)) {
                        return;
                    }
                    graphics.DrawLine(linePen, p1, p2);
                }
                void drawCircle(PointF p) {
                    graphics.FillEllipse(circleBrush, p.X, p.Y, circleRadius, circleRadius);
                }
                if (DrawHeadLandmarks) {
                    foreach (var p in datum.Pose.VisiableLandmarks) {
                        var point = new PointF(p.X, p.Y);
                        drawCircle(point);
                    } 
                }
                if (DrawEyeLandmarks) {
                    foreach (var p in datum.Eye.VisiableLandmarks) {
                        var point = new PointF(p.X, p.Y);
                        drawCircle(point);
                    } 
                }
                if (DrawHeadIndicatorLines) {
                    foreach (var l in datum.Pose.IndicatorLines) {
                        var a = new PointF(l.Item1.X, l.Item1.Y);
                        var b = new PointF(l.Item2.X, l.Item2.Y);
                        drawLine(a, b);
                    } 
                }
                if (DrawEyeIndicatorLines) {
                    foreach (var l in datum.Eye.IndicatorLines) {
                        var a = new PointF(l.Item1.X, l.Item1.Y);
                        var b = new PointF(l.Item2.X, l.Item2.Y);
                        drawLine(a, b);
                    } 
                }
                using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
                img.Resource.CopyFrom(bitmap);
                imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
                Out.Post(img, envelope.OriginatingTime);
            }
        }

        public void RenderingCallback(object sender, EventArgs args) => imageVisualizer.RenderingCallback(sender, args);
    }
}
