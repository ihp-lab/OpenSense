using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenFaceInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSense.Components {

    public class FlipColorVideo : IConsumer<Shared<Image>>, IProducer<Shared<Image>> {

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        public bool FlipHorizontal { get; set; } = Properties.Settings.Default.VideoDeviceHFlip;
        public bool FlipVertical { get; set; } = Properties.Settings.Default.VideoDeviceVFlip;

        public FlipColorVideo(Pipeline pipeline) {
            // psi pipeline
            In = pipeline.CreateReceiver<Shared<Image>>(this, PorcessFrame, nameof(In));
            Out = pipeline.CreateEmitter<Shared<Image>>(this, nameof(Out));
        }

        private void PorcessFrame(Shared<Image> frame, Envelope envelope) {
            if (!FlipHorizontal && !FlipVertical) {
                Out.Post(frame, envelope.OriginatingTime);
                return;
            }
            //Note: do not use Shared<Image>.Resource.Flip, because it will change image format from 24bpp to 32bpp
            using (var result = ImagePool.GetOrCreate(frame.Resource.Width, frame.Resource.Height, frame.Resource.PixelFormat)) {
                frame.Resource.CopyTo(result.Resource);
                var resultMeta = new ImageBuffer(result.Resource.Width, result.Resource.Height, result.Resource.ImageData, result.Resource.Stride);
                if (FlipHorizontal) {
                    Methods.FlipHorizontally(resultMeta, resultMeta);
                }
                if (FlipVertical) {
                    Methods.FlipVertically(resultMeta, resultMeta);
                }
                Debug.Assert(result.Resource.PixelFormat == frame.Resource.PixelFormat);
                Out.Post(result, envelope.OriginatingTime);
            }
        }
    }
}
