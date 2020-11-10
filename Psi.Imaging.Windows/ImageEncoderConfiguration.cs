using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Imaging {
    [Serializable]
    public class ImageEncoderConfiguration : ConventionalComponentConfiguration {

        private PsiBuiltinImageToStreamEncoder encoder = PsiBuiltinImageToStreamEncoder.Png;

        public PsiBuiltinImageToStreamEncoder Encoder {
            get => encoder;
            set => SetProperty(ref encoder, value);
        }

        private int qualityLevel = 100;

        public int QualityLevel {
            get => qualityLevel;
            set => SetProperty(ref qualityLevel, value);
        }

        private IImageToStreamEncoder CreateEncoder() {
            switch (Encoder) {
                case PsiBuiltinImageToStreamEncoder.Png:
                    return new ImageToPngStreamEncoder();
                case PsiBuiltinImageToStreamEncoder.Jpeg:
                    return new ImageToJpegStreamEncoder() { QualityLevel = QualityLevel };
                default:
                    throw new InvalidOperationException();
            }
        }

        public override IComponentMetadata GetMetadata() => new ImageEncoderMetadata();

        protected override object Instantiate(Pipeline pipeline) => new ImageEncoder(pipeline, CreateEncoder());
    }
}
