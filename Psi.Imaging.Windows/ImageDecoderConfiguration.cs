using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Psi.Imaging {
    [Serializable]
    public class ImageDecoderConfiguration : ConventionalComponentConfiguration {

        private IImageFromStreamDecoder CreateDecoder() {
            return new ImageFromStreamDecoder();//default impl in Windows
        }

        public override IComponentMetadata GetMetadata() => new ImageDecoderMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ImageDecoder(pipeline, CreateDecoder());
    }
}
