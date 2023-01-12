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

namespace OpenSense.Components.OpenPose.Visualizer {
    public class OpenPoseVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

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
        private Connector<Datum> DataInConnector;

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<Datum> DataIn => DataInConnector.In;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

        public Emitter<Shared<Image>> Out { get; private set; }
        #endregion

        #region Settings
        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private bool drawPose = true;

        public bool DrawPose {
            get => drawPose;
            set => SetProperty(ref drawPose, value);
        }

        private bool drawFace = true;

        public bool DrawFace {
            get => drawFace;
            set => SetProperty(ref drawFace, value);
        }

        private bool drawHand = true;

        public bool DrawHand {
            get => drawHand;
            set => SetProperty(ref drawHand, value);
        }

        private bool NoDraw => !(DrawPose || DrawFace || DrawHand);

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

        public OpenPoseVisualizer(Pipeline pipeline): base(pipeline) {
            DataInConnector = CreateInputConnectorFrom<Datum>(pipeline, nameof(DataIn));
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined = DataInConnector.Out.Join(ImageInConnector.Out, Reproducible.Nearest<Shared<Image>>());
            joined.Do(Process);

            imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        private void Process(ValueTuple<Datum, Shared<Image>> data, Envelope envelope) {
            if (Mute) {
                return;
            }
            var (datum, frame) = data;
            if (NoDraw) {
                imageVisualizer.UpdateImage(frame, envelope.OriginatingTime);
                Out.Post(frame, envelope.OriginatingTime);
                return;
            }
            lock (this) {
                //draw
                //https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/output.md#face-output-format
                if (frame?.Resource != null) {
                    var bitmap = frame.Resource.ToBitmap();
                    using var linePen = new Pen(Color.LightGreen, LineThickness);
                    using var circleBrush = new SolidBrush(Color.LightGreen);
                    using var graphics = Graphics.FromImage(bitmap);
                    void drawLine(PointF p1, PointF p2) {
                        if ((p1.X != 0 || p1.Y != 0) && (p2.X != 0 || p2.Y != 0)) {
                            graphics.DrawLine(linePen, p1, p2);
                        }
                    }
                    void drawCircle(PointF p) {
                        graphics.FillEllipse(circleBrush, p.X, p.Y, circleRadius, circleRadius);
                    }
                    #region draw pose
                    if (DrawPose && datum.poseKeypoints != null) {
                        for (var people = 0; people < datum.poseKeypoints.GetSize(0); people++) {
                            PointF getPoint(int part) {
                                var x = datum.poseKeypoints.Get(people, part, 0);
                                var y = datum.poseKeypoints.Get(people, part, 1);
                                var score = datum.poseKeypoints.Get(people, part, 2);
                                return new PointF(x, y);
                            }
                            void drawPoseLine(int part1, int part2) {
                                drawLine(getPoint(part1), getPoint(part2));
                            }

                            //BODY_25
                            for (var i = 0; i < 25; i++) {
                                drawCircle(getPoint(i));
                            }

                            drawPoseLine(0, 1);
                            drawPoseLine(1, 2);
                            drawPoseLine(2, 3);
                            drawPoseLine(3, 4);
                            drawPoseLine(1, 5);
                            drawPoseLine(5, 6);
                            drawPoseLine(6, 7);
                            drawPoseLine(1, 8);
                            drawPoseLine(8, 9);
                            drawPoseLine(9, 10);
                            drawPoseLine(10, 11);
                            drawPoseLine(8, 12);
                            drawPoseLine(12, 13);
                            drawPoseLine(13, 14);
                            drawPoseLine(0, 15);
                            drawPoseLine(0, 16);
                            drawPoseLine(15, 17);
                            drawPoseLine(16, 18);
                            drawPoseLine(19, 20);
                            drawPoseLine(19, 21);
                            drawPoseLine(14, 21);
                            drawPoseLine(22, 23);
                            drawPoseLine(22, 24);
                            drawPoseLine(11, 24);
                        }
                    }
                    #endregion
                    
                    #region draw face
                    if (DrawFace && datum.faceKeypoints != null) {
                        for (var people = 0; people < datum.faceKeypoints.GetSize(0); people++) {
                            PointF getPoint(int part) {
                                var x = datum.faceKeypoints.Get(people, part, 0);
                                var y = datum.faceKeypoints.Get(people, part, 1);
                                var score = datum.faceKeypoints.Get(people, part, 2);
                                return new PointF(x, y);
                            }
                            void drawFaceLine(int part1, int part2) {
                                drawLine(getPoint(part1), getPoint(part2));
                            }

                            for (var i = 0; i < 70; i++) {
                                drawCircle(getPoint(i));
                            }

                            drawFaceLine(31, 32);
                            drawFaceLine(32, 33);
                            drawFaceLine(33, 34);
                            drawFaceLine(34, 35);

                            drawFaceLine(27, 28);
                            drawFaceLine(28, 29);
                            drawFaceLine(29, 30);

                            drawFaceLine(36, 37);
                            drawFaceLine(37, 38);
                            drawFaceLine(38, 39);
                            drawFaceLine(39, 40);
                            drawFaceLine(40, 41);
                            drawFaceLine(41, 36);

                            drawFaceLine(42, 43);
                            drawFaceLine(43, 44);
                            drawFaceLine(44, 45);
                            drawFaceLine(45, 46);
                            drawFaceLine(46, 47);
                            drawFaceLine(47, 42);

                            drawFaceLine(48, 49);
                            drawFaceLine(49, 50);
                            drawFaceLine(50, 51);
                            drawFaceLine(51, 52);
                            drawFaceLine(52, 53);
                            drawFaceLine(53, 54);
                            drawFaceLine(54, 55);
                            drawFaceLine(55, 56);
                            drawFaceLine(56, 57);
                            drawFaceLine(57, 58);
                            drawFaceLine(58, 59);
                            drawFaceLine(59, 48);

                            drawFaceLine(60, 61);
                            drawFaceLine(61, 62);
                            drawFaceLine(62, 63);
                            drawFaceLine(63, 64);
                            drawFaceLine(64, 65);
                            drawFaceLine(65, 66);
                            drawFaceLine(66, 67);
                            drawFaceLine(67, 60);

                            drawFaceLine(17, 18);
                            drawFaceLine(18, 19);
                            drawFaceLine(19, 20);
                            drawFaceLine(20, 21);

                            drawFaceLine(22, 23);
                            drawFaceLine(23, 24);
                            drawFaceLine(24, 25);
                            drawFaceLine(25, 26);

                            drawFaceLine(0, 1);
                            drawFaceLine(1, 2);
                            drawFaceLine(2, 3);
                            drawFaceLine(3, 4);
                            drawFaceLine(4, 5);
                            drawFaceLine(5, 6);
                            drawFaceLine(6, 7);
                            drawFaceLine(7, 8);
                            drawFaceLine(8, 9);
                            drawFaceLine(9, 10);
                            drawFaceLine(10, 11);
                            drawFaceLine(11, 12);
                            drawFaceLine(12, 13);
                            drawFaceLine(13, 14);
                            drawFaceLine(14, 15);
                            drawFaceLine(15, 16);
                        }
                    }
                    #endregion

                    #region draw hand
                    if (DrawHand && datum.handKeypoints != null) {
                        for (var people = 0; people < datum.faceKeypoints.GetSize(0); people++) {
                            PointF getLeftPoint(int part) {
                                var x = datum.handKeypoints.left.Get(people, part, 0);
                                var y = datum.handKeypoints.left.Get(people, part, 1);
                                var score = datum.handKeypoints.left.Get(people, part, 2);
                                return new PointF(x, y);
                            }
                            PointF getRightPoint(int part) {
                                var x = datum.handKeypoints.right.Get(people, part, 0);
                                var y = datum.handKeypoints.right.Get(people, part, 1);
                                var score = datum.handKeypoints.right.Get(people, part, 2);
                                return new PointF(x, y);
                            }
                            void drawDoubleHandLine(int part1, int part2) {
                                drawLine(getLeftPoint(part1), getLeftPoint(part2));
                                drawLine(getRightPoint(part1), getRightPoint(part2));
                            }

                            for (var i = 0; i < 21; i++) {
                                drawCircle(getLeftPoint(i));
                                drawCircle(getRightPoint(i));
                            }

                            drawDoubleHandLine(0, 1);
                            drawDoubleHandLine(1, 2);
                            drawDoubleHandLine(2, 3);
                            drawDoubleHandLine(3, 4);

                            drawDoubleHandLine(0, 5);
                            drawDoubleHandLine(5, 6);
                            drawDoubleHandLine(6, 7);
                            drawDoubleHandLine(7, 8);

                            drawDoubleHandLine(0, 9);
                            drawDoubleHandLine(9, 10);
                            drawDoubleHandLine(10, 11);
                            drawDoubleHandLine(11, 12);

                            drawDoubleHandLine(0, 13);
                            drawDoubleHandLine(13, 14);
                            drawDoubleHandLine(14, 15);
                            drawDoubleHandLine(15, 16);

                            drawDoubleHandLine(0, 17);
                            drawDoubleHandLine(17, 18);
                            drawDoubleHandLine(18, 19);
                            drawDoubleHandLine(19, 20);
                        }
                    }
                    #endregion
                    using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
                    img.Resource.CopyFrom(bitmap);
                    imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
                    Out.Post(img, envelope.OriginatingTime);
                }
            }
        }

        public void RenderingCallback(object sender, EventArgs args) => imageVisualizer.RenderingCallback(sender, args);
    }
}
