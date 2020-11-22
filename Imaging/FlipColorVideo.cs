using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenCvSharp.Extensions;

namespace OpenSense.Component.Imaging {

    public class FlipColorVideo : IConsumer<Shared<Image>>, IProducer<Shared<Image>>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        private bool flipHorizontal = false;

        public bool FlipHorizontal {
            get => flipHorizontal;
            set => SetProperty(ref flipHorizontal, value);
        }

        private bool flipVertical = false;

        public bool FlipVertical {
            get => flipVertical;
            set => SetProperty(ref flipVertical, value);
        }

        public FlipColorVideo(Pipeline pipeline) {
            // psi pipeline
            In = pipeline.CreateReceiver<Shared<Image>>(this, PorcessFrame, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private void PorcessFrame(Shared<Image> frame, Envelope envelope) {
            var flip = (FlipHorizontal, FlipVertical);
            if (flip == (false, false)) {
                Out.Post(frame, envelope.OriginatingTime);
                return;
            }
            //Note: do not use Shared<Image>.Resource.Flip, because it will change image format from 24bpp to 32bpp
            var bitmap = frame.Resource.ToBitmap(makeCopy:true);
            try {//native opencv
                var mat = bitmap.ToMat();//data copied
                mat = flip switch {
                    (true, true) => mat.Flip(OpenCvSharp.FlipMode.XY),
                    (true, false) => mat.Flip(OpenCvSharp.FlipMode.Y),
                    (false, true) => mat.Flip(OpenCvSharp.FlipMode.X),
                    _ => throw new InvalidOperationException("This statement should not be executed"),
                };
                mat.ToBitmap(bitmap);
            } catch (Exception ex) {
                (FlipHorizontal, FlipVertical) = (false, false);
                throw;
            }
            using var result = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat);
            result.Resource.CopyFrom(bitmap);
            Debug.Assert(result.Resource.PixelFormat == frame.Resource.PixelFormat);
            Out.Post(result, envelope.OriginatingTime);
        }
    }

}
