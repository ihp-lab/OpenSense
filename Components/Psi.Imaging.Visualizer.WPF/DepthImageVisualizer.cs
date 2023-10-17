using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;
using OpenSense.WPF.Components.Psi.Imaging.Visualizer;

namespace OpenSense.Components.Psi.Imaging.Visualizer {
    public sealed class DepthImageVisualizer : IConsumerProducer<Shared<DepthImage>, Shared<Image>>, INotifyPropertyChanged {

        #region Ports
        public Receiver<Shared<DepthImage>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; } 
        #endregion

        #region Settings
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
        #endregion

        #region Binding Properties
        public WriteableBitmap Image => imageVisualizer.Image;

        public double? FrameRate => imageVisualizer.FrameRate;
        #endregion

        private ImageHolder imageVisualizer = new ImageHolder();

        public DepthImageVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<DepthImage>>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));

            imageVisualizer.PropertyChanged += (sender, e) => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
            };
        }

        public void RenderingCallback(object sender, EventArgs args) => imageVisualizer.RenderingCallback(sender, args);

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
            using var img = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, PixelFormat.BGRA_32bpp);
            frame.Resource.PseudoColorize(img.Resource, (MinValue, MaxValue));
            imageVisualizer.UpdateImage(img, envelope.OriginatingTime);
            Out.Post(img, envelope.OriginatingTime);
        }

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
