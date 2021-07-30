using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging {
    [Export(typeof(IComponentMetadata))]
    public class PixelFormatConverterMetadata : ConventionalComponentMetadata {

        public override string Description => "Convert image pixel format.";

        protected override Type ComponentType => typeof(PixelFormatConverter);

        public override ComponentConfiguration CreateConfiguration() => new PixelFormatConverterConfiguration();
    }
}
