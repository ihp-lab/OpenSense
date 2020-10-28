using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Component.Imaging {

    public class FlipColorVideo : IConsumer<Shared<Image>>, IProducer<Shared<Image>> {

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<Shared<Image>> Out { get; private set; }

        public bool FlipHorizontal { get; set; } = false;
        public bool FlipVertical { get; set; } = false;

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
                /* Remove dependency of OpenFaceInterop (ImageBuffer)
                frame.Resource.CopyTo(result.Resource);
                var resultMeta = new ImageBuffer(result.Resource.Width, result.Resource.Height, result.Resource.ImageData, result.Resource.Stride);
                if (FlipHorizontal) {
                    Methods.FlipHorizontally(resultMeta, resultMeta);
                }
                if (FlipVertical) {
                    Methods.FlipVertically(resultMeta, resultMeta);
                }
                Debug.Assert(result.Resource.PixelFormat == frame.Resource.PixelFormat);
                */
                Out.Post(result, envelope.OriginatingTime);
            }
        }
    }

}
