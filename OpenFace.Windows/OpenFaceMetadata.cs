using System;
using System.Composition;
using OpenSense.Components.Contract;

namespace OpenSense.Components.OpenFace {
    [Export(typeof(IComponentMetadata))]
    public class OpenFaceMetadata : ConventionalComponentMetadata {

        public override string Description => "OpenFace by MultiComp Lab for head detection. This wapper of OpenFace can detect up to 1 person.";

        protected override Type ComponentType => typeof(OpenFace);

        public override string Name => nameof(OpenFace);

        protected override string GetPortDescription(string portName) {
            switch (portName) {
                case nameof(OpenFace.In):
                    return "[Required] Images.";
                case nameof(OpenFace.Out):
                    return "[Composite] All kinds of outputs combined together.";
                case nameof(OpenFace.PoseOut):
                    return "[Composite] Head pose results. Contains head transform and landmarks.";
                case nameof(OpenFace.EyeOut):
                    return "[Composite] Eye detection results. Contains eye transforms and landmarks.";
                case nameof(OpenFace.FaceOut):
                    return "[Composite] Face output results. Contains action units.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new OpenFaceConfiguration();
    }
}
