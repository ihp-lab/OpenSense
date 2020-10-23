using Microsoft.Psi;
using Microsoft.Psi.Common.Interpolators;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.Utilities;
using OpenFaceInterop;
using OpenPose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using OpenSense.DataStructure;

namespace OpenSense.Components.Display {
    public class OpenFaceVisualizer : Subpipeline, IProducer<Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Connector<HeadPoseAndGaze> DataInConnector;

        private Connector<Shared<Image>> ImageInConnector;

        public Receiver<HeadPoseAndGaze> DataIn => DataInConnector.In;

        public Receiver<Shared<Image>> ImageIn => ImageInConnector.In;

        public Emitter<Shared<Image>> Out { get; private set; }

        public OpenFaceVisualizer(Pipeline pipeline) : base(pipeline) {
            DataInConnector = CreateInputConnectorFrom<HeadPoseAndGaze>(pipeline, nameof(DataIn));
            ImageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            var joined = DataInConnector.Out.Join(ImageInConnector.Out, Reproducible.Exact<Shared<Image>>());
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

        private void Process(ValueTuple<HeadPoseAndGaze, Shared<Image>> data, Envelope envelope) {
            var (datum, frame) = data;
            lock (this) {
                if (frame != null && frame.Resource != null) {
                    using (var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat)) {
                        frame.Resource.CopyTo(img.Resource);
                        var buffer = new ImageBuffer(img.Resource.Width, img.Resource.Height, img.Resource.ImageData, img.Resource.Stride);
                        foreach (var p in datum.HeadPose.VisiableLandmarks) {
                            Methods.DrawPoint(buffer, new Point(p.X, p.Y), 3);
                        }

                        foreach (var p in datum.Gaze.VisiableLandmarks) {
                            Methods.DrawPoint(buffer, new Point(p.X, p.Y), 1);
                        }

                        foreach (var l in datum.HeadPose.IndicatorLines) {
                            Methods.DrawLine(buffer, new Point(l.Item1.X, l.Item1.Y), new Point(l.Item2.X, l.Item2.Y));
                        }

                        foreach (var l in datum.Gaze.IndicatorLines) {
                            Methods.DrawLine(buffer, new Point(l.Item1.X, l.Item1.Y), new Point(l.Item2.X, l.Item2.Y));
                        }

                        Out.Post(img, envelope.OriginatingTime);
                        display.Update(img);
                    }
                }
            }
        }

        private void OnPipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }

        private DisplayVideo display = new DisplayVideo();

        public WriteableBitmap Image {
            get => display.VideoImage;
        }

        public int FrameRate {
            get => display.ReceivedFrames.Rate;
        }
    }
}
