using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Mediapipe.Net.Framework.Protobuf;
using Mediapipe.Net.Solutions;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Imaging.Visualizer.Common;
using OpenSense.Component.MediaPipe.NET;
using Color = System.Drawing.Color;
using Image = Microsoft.Psi.Imaging.Image;

namespace OpenSense.Wpf.Component.MediaPipe.NET.Visualizer {
    /// <remarks>Base on <see href="https://github.com/google/mediapipe/blob/26a7ca5c64cd885978677931a7218d33cd7d1dec/mediapipe/python/solutions/drawing_utils.py#L119">draw_landmarks()</see>.</remarks>
    public sealed class NormalizedLandmarkListVectorVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

        #region Ports
        private Connector<IReadOnlyList<NormalizedLandmarkList>> DataInConnector;

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => DataInConnector.In;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

        public Emitter<Shared<Image>> Out { get; private set; }
        #endregion

        #region Settings
        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        //private float presenceThreshold = 0.5f;

        //public float PresenceThreshold {
        //    get => presenceThreshold;
        //    set => SetProperty(ref presenceThreshold, value);
        //}

        //private float visibilityThreshold = 0.5f;

        //public float VisibilityThreshold {
        //    get => visibilityThreshold;
        //    set => SetProperty(ref visibilityThreshold, value);
        //}

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

        private bool drawLandmarks = true;

        public bool DrawLandmarks {
            get => drawLandmarks;
            set => SetProperty(ref drawLandmarks, value);
        }

        private bool drawLips = false;

        public bool DrawLips {
            get => drawLips;
            set => SetProperty(ref drawLips, value);
        }

        private bool drawLeftEye = false;

        public bool DrawLeftEye {
            get => drawLeftEye;
            set => SetProperty(ref drawLeftEye, value);
        }

        private bool drawRightEye = false;

        public bool DrawRightEye {
            get => drawRightEye;
            set => SetProperty(ref drawRightEye, value);
        }

        private bool drawLeftEyebrow = false;

        public bool DrawLeftEyebrow {
            get => drawLeftEyebrow;
            set => SetProperty(ref drawLeftEyebrow, value);
        }

        private bool drawRightEyebrow = false;

        public bool DrawRightEyebrow {
            get => drawRightEyebrow;
            set => SetProperty(ref drawRightEyebrow, value);
        }

        private bool drawLeftIris = false;

        public bool DrawLeftIris {
            get => drawLeftIris;
            set => SetProperty(ref drawLeftIris, value);
        }

        private bool drawRightIris = false;

        public bool DrawRightIris {
            get => drawRightIris;
            set => SetProperty(ref drawRightIris, value);
        }

        private bool drawFaceOval = false;

        public bool DrawFaceOval {
            get => drawFaceOval;
            set => SetProperty(ref drawFaceOval, value);
        }

        private bool drawTesselation = false;

        public bool DrawTesselation {
            get => drawTesselation;
            set => SetProperty(ref drawTesselation, value);
        }

        #endregion

        #region Binding Properties
        public WriteableBitmap Image => imageVisualizer.Image;

        public double? FrameRate => imageVisualizer.FrameRate;
        #endregion

        private ImageVisualizer imageVisualizer = new ImageVisualizer();

        public NormalizedLandmarkListVectorVisualizer(Pipeline pipeline, string name = null, DeliveryPolicy defaultDeliveryPolicy = null) : base(pipeline, name, defaultDeliveryPolicy) {
            DataInConnector = CreateInputConnectorFrom<IReadOnlyList<NormalizedLandmarkList>>(pipeline, nameof(DataIn));
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined = DataInConnector.Out.Join(ImageInConnector.Out, Reproducible.Exact<Shared<Image>>());
            joined.Do(Process);

            imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        private void Process(ValueTuple<IReadOnlyList<NormalizedLandmarkList>, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            var (datum, frame) = data;
            if (frame?.Resource != null) {
                var bitmap = frame.Resource.ToBitmap();
                using var linePen = new Pen(Color.LightGreen, LineThickness);
                using var circleBrush = new SolidBrush(Color.LightGreen);
                using var graphics = Graphics.FromImage(bitmap);
                PointF? convertToPointF(NormalizedLandmark landmark) {
                    //No Presence & Visibility?
                    //if (landmark.Visibility < VisibilityThreshold || landmark.Presence < PresenceThreshold) {
                    //    continue;
                    //}
                    Debug.Assert(landmark.HasX);
                    Debug.Assert(landmark.HasY);
                    var xNorm = landmark.X;
                    var yNorm = landmark.Y;
                    if (!IsNormalized(xNorm) || !IsNormalized(yNorm)) {
                        return null;
                    }
                    var x = Math.Min(MathF.Floor(xNorm * frame.Resource.Width), frame.Resource.Width - 1);
                    var y = Math.Min(MathF.Floor(yNorm * frame.Resource.Height), frame.Resource.Height - 1);
                    var result = new PointF(x, y);
                    return result;
                }
                void drawCircle(PointF p) {
                    graphics.FillEllipse(circleBrush, p.X, p.Y, circleRadius, circleRadius);
                }
                void drawLine(PointF p1, PointF p2) {
                    if ((p1.X == 0 && p1.Y == 0) || (p2.X == 0 && p2.Y == 0)) {
                        return;
                    }
                    graphics.DrawLine(linePen, p1, p2);
                }
                void drawConnections(IReadOnlyCollection<ValueTuple<int, int>> connections) {
                    foreach (var person in datum) {
                        foreach (var (idx1, idx2) in connections) {
                            if (idx1 >= person.Landmark.Count || idx2 >= person.Landmark.Count) {
                                continue;
                            }
                            var landmark1 = person.Landmark[idx1];
                            var landmark2 = person.Landmark[idx2];
                            var point1 = convertToPointF(landmark1);
                            var point2 = convertToPointF(landmark2);
                            if (point1 is PointF p1 && point2 is PointF p2) {
                                drawLine(p1, p2);
                            }
                        }
                    }
                }
                if (DrawLandmarks) {
                    foreach (var person in datum) {
                        foreach (var landmark in person.Landmark) {
                            var point = convertToPointF(landmark);
                            if (point is PointF p) {
                                drawCircle(p);
                            }
                        }
                    }
                }
                if (DrawLips) {
                    drawConnections(FaceMeshConnectionConstants.Lips);
                }
                if (DrawLeftEye) {
                    drawConnections(FaceMeshConnectionConstants.LeftEye);
                }
                if (DrawLeftEyebrow) {
                    drawConnections(FaceMeshConnectionConstants.LeftEyebrow);
                }
                if (DrawLeftIris) {
                    drawConnections(FaceMeshConnectionConstants.LeftIris);
                }
                if (DrawRightEye) {
                    drawConnections(FaceMeshConnectionConstants.RightEye);
                }
                if (DrawRightEyebrow) {
                    drawConnections(FaceMeshConnectionConstants.RightEyebrow);
                }
                if (DrawRightIris) {
                    drawConnections(FaceMeshConnectionConstants.RightIris);
                }
                if (DrawFaceOval) {
                    drawConnections(FaceMeshConnectionConstants.FaceOval);
                }
                if (DrawTesselation) {
                    drawConnections(FaceMeshConnectionConstants.Tesselation);
                }

                using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
                img.Resource.CopyFrom(bitmap);
                imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
                Out.Post(img, envelope.OriginatingTime);
            }
        }

        private static bool IsNormalized(float value) => 0 <= value && value <= 1;

        public void RenderingCallback(object sender, EventArgs args) => imageVisualizer.RenderingCallback(sender, args);

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
