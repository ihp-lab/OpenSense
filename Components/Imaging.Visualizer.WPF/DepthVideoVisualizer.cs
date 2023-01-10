using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Imaging.Visualizer {
    public class DepthVideoVisualizer : IConsumerProducer<Shared<DepthImage>, Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private DisplayVideo display = new DisplayVideo();

        public Receiver<Shared<DepthImage>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        public WriteableBitmap Image {
            get => display.VideoImage;
        }

        public int FrameRate {
            get => display.ReceivedFrames.Rate;
        }

        private bool autoExpandRange = true;

        public bool AutoExpandRange {
            get => autoExpandRange;
            set => SetProperty(ref autoExpandRange, value);
        }

        private ushort minValue = ushort.MaxValue;

        public ushort MinValue {
            get => minValue;
            set => SetProperty(ref minValue, value);
        }

        private ushort maxValue = ushort.MinValue;

        public ushort MaxValue {
            get => maxValue;
            set => SetProperty(ref maxValue, value);
        }

        public DepthVideoVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<DepthImage>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
            pipeline.PipelineCompleted += PipelineCompleted;

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

        private void Process(Shared<DepthImage> frame, Envelope envelope) {
            if (AutoExpandRange) {
                Debug.Assert(frame.Resource.PixelFormat == PixelFormat.Gray_16bpp);
                var ptr = frame.Resource.ImageData;
                for (var i = 0; i < frame.Resource.Height; i++) {
                    var offset = i * frame.Resource.Stride;
                    for (var j = 0; j < frame.Resource.Width; j++) {
                        var val = (ushort)Marshal.ReadInt16(ptr, offset);
                        if (val != 0) {
                            if (val < MinValue) {
                                MinValue = val;
                            }
                            if (val > MaxValue) {
                                MaxValue = val;
                            }
                        }
                        offset += sizeof(ushort);
                    }
                }
            }
            using (var color = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, PixelFormat.BGR_24bpp)) {
                frame.Resource.PseudoColorize(color.Resource, (MinValue, MaxValue));
                Out.Post(color, envelope.OriginatingTime);
                display.Update(color);
            }
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            display.Clear();
        }
    }
}
