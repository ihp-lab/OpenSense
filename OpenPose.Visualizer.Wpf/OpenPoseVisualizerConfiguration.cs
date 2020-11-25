using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenPose.Visualizer {
    [Serializable]
    public class OpenPoseVisualizerConfiguration : ConventionalComponentConfiguration {

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private int jointRadius = 3;

        public int JointRadius {
            get => jointRadius;
            set => SetProperty(ref jointRadius, value);
        }

        private int boneThickness = 1;

        public int BoneThickness {
            get => boneThickness;
            set => SetProperty(ref boneThickness, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenPoseVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new OpenPoseVisualizer(pipeline) {
            Mute = Mute,
            BoneThickness = BoneThickness,
            JointRadius = JointRadius,
        };
    }
}
