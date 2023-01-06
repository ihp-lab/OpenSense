using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Imaging {
    public class PixelFormatConverter: IConsumer<Shared<Image>>, IProducer<Shared<Image>>, INotifyPropertyChanged {

        private PixelFormat targetPixelFormat = PixelFormat.BGR_24bpp;

        public PixelFormat TargetPixelFormat {
            get => targetPixelFormat;
            set => SetProperty(ref targetPixelFormat, value);
        }

        private bool bypassIfPossible = true;

        public bool BypassIfPossible {
            get => bypassIfPossible;
            set => SetProperty(ref bypassIfPossible, value);
        }

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        public PixelFormatConverter(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, PorcessFrame, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private void PorcessFrame(Shared<Image> frame, Envelope envelope) {
            if (BypassIfPossible && frame.Resource.PixelFormat == TargetPixelFormat) {
                Out.Post(frame, envelope.OriginatingTime);
                return;
            }
            var result = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, TargetPixelFormat);
            frame.Resource.CopyTo(result.Resource);//psi internal implementation: https://github.com/microsoft/psi/blob/master/Sources/Imaging/Microsoft.Psi.Imaging/ToPixelFormat.cs
            Out.Post(frame, envelope.OriginatingTime);
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
