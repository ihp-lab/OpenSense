using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.HeadGesture {
    [Serializable]
    public class HeadGestureDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new HeadGestureDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HeadGestureDetector(pipeline);
    }
}
