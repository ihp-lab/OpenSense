using System;
using Microsoft.Psi;

namespace OpenSense.Components.LibreFace {
    [Serializable]
    public class LibreFaceDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new LibreFaceDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new LibreFaceDetector(pipeline) {
            
        };
    }
}
