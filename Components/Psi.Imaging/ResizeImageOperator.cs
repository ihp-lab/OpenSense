using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Psi.Imaging {
    public sealed class ResizeImageOperator : IConsumer<Shared<Image>>, IProducer<Shared<Image>>, INotifyPropertyChanged {

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        private int width = 640;

        public int Width {
            get => width;
            set => SetProperty(ref width, value);
        }

        private int height = 480;

        public int Height {
            get => height;
            set => SetProperty(ref height, value);
        }

        private SamplingMode samplingMode = SamplingMode.Bilinear;

        public SamplingMode SamplingMode {
            get => samplingMode;
            set => SetProperty(ref samplingMode, value);
        }

        private bool bypassIfPossible = true;

        public bool BypassIfPossible {
            get => bypassIfPossible;
            set => SetProperty(ref bypassIfPossible, value);
        }

        public ResizeImageOperator(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, PorcessFrame, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private void PorcessFrame(Shared<Image> frame, Envelope envelope) {
            if (BypassIfPossible && frame.Resource.Width == Width && frame.Resource.Height == Height) {
                Out.Post(frame, envelope.OriginatingTime);
                return;
            }
            using var result = ImagePool.GetOrCreate(Width, Height, frame.Resource.PixelFormat);
            frame.Resource.Resize(result.Resource, Width, Height, SamplingMode);
            Out.Post(result, envelope.OriginatingTime);
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
