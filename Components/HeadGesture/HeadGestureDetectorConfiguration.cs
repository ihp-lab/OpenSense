using System;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.HeadGesture {
    [Serializable]
    public class HeadGestureDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new HeadGestureDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HeadGestureDetector(pipeline);
    }
}
