using System;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.HeadGesture {
    [Export(typeof(IComponentMetadata))]
    public class HeadGestureDetectorMetadata : ConventionalComponentMetadata {

        public override string Description => "Detect head gesture from given head pose information.";

        protected override Type ComponentType => typeof(HeadGestureDetector);

        public override ComponentConfiguration CreateConfiguration() => new HeadGestureDetectorConfiguration();
    }
}
