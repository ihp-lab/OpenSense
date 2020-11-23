using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.AzureKinect.Visualizer {
    [Serializable]
    public class AzureKinectBodyTrackerVisualizerConfiguration : ConventionalComponentConfiguration {

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

        public override IComponentMetadata GetMetadata() => new AzureKinectBodyTrackerVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new AzureKinectBodyTrackerVisualizer(pipeline) {
            Mute = Mute,
            BoneThickness = BoneThickness,
            JointRadius = JointRadius,
        };
    }
}
