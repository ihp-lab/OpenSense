using System;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging {
    [Serializable]
    public class PixelFormatConverterConfiguration : ConventionalComponentConfiguration {

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

        public override IComponentMetadata GetMetadata() => new PixelFormatConverterMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new PixelFormatConverter(pipeline) { 
            TargetPixelFormat = TargetPixelFormat,
            BypassIfPossible = BypassIfPossible,
        };
    }
}
