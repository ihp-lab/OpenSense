using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenSense.Component.Imaging.Visualizer.Common;
using OpenSense.Component.OpenPose.Common;

namespace OpenSense.Component.OpenPose.Visualizer {
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

        private Connector<Datum> DataInConnector;

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<Datum> DataIn => DataInConnector.In;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

        public Emitter<Shared<Image>> Out {get; private set;}

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

        private DisplayVideo display = new DisplayVideo();

        public WriteableBitmap Image {
            get => display.VideoImage;
        }

        public int FrameRate {
            get => display.ReceivedFrames.Rate;
        }

        public OpenPoseVisualizer(Pipeline pipeline): base(pipeline) {
            DataInConnector = CreateInputConnectorFrom<Datum>(pipeline, nameof(DataIn));
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined = DataInConnector.Out.Join(ImageInConnector.Out, Reproducible.Nearest<Shared<Image>>());
            joined.Do(Process);
            
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

        private void Process(ValueTuple<Datum, Shared<Image>> data, Envelope envelope) {
            var (datum, frame) = data;
            lock (this) {
                //draw
                if (frame != null && frame.Resource != null) {
                    var bitmap = frame.Resource.ToBitmap();
                    try {//native opencv
                        var mat = bitmap.ToMat();//data copied
                        #region draw pose
                        if (datum.poseKeypoints != null) {
                            for (var people = 0; people < datum.poseKeypoints.GetSize(0); people++) {
                                OpenCvSharp.Point GetPoint(int bodyPart) {
                                    var x = datum.poseKeypoints.Get(people, bodyPart, 0);
                                    var y = datum.poseKeypoints.Get(people, bodyPart, 1);
                                    var score = datum.poseKeypoints.Get(people, bodyPart, 2);
                                    return new OpenCvSharp.Point(x, y);
                                }
                                void Line(int partIdx1, int partIdx2) {
                                    var p1 = GetPoint(partIdx1);
                                    var p2 = GetPoint(partIdx2);
                                    if ((p1.X != 0 || p1.Y != 0) && (p2.X != 0 || p2.Y != 0)) {
                                        mat.Line(p1, p2, Scalar.LightGreen, BoneThickness, lineType: LineTypes.Link8);
                                    }
                                }
                                //BODY_25
                                for (var i = 0; i < 25; i++) {
                                    mat.Circle(GetPoint(i), JointRadius, Scalar.LightGreen, thickness: -1, lineType: LineTypes.Link8);
                                }
                                Line(0, 1);
                                Line(1, 2);
                                Line(2, 3);
                                Line(3, 4);
                                Line(1, 5);
                                Line(5, 6);
                                Line(6, 7);
                                Line(1, 8);
                                Line(8, 9);
                                Line(9, 10);
                                Line(10, 11);
                                Line(8, 12);
                                Line(12, 13);
                                Line(13, 14);
                                Line(0, 15);
                                Line(0, 16);
                                Line(15, 17);
                                Line(16, 18);
                                Line(19, 20);
                                Line(19, 21);
                                Line(14, 21);
                                Line(22, 23);
                                Line(22, 24);
                                Line(11, 24);
                            }
                        }
                        #endregion
                        //draw face // TODO:
                        //draw hand // TODO:
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
