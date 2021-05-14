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

        private int circleRadius = 3;

        public int CircleRadius {
            get => circleRadius;
            set => SetProperty(ref circleRadius, value);
        }

        private int lineThickness = 1;

        public int LineThickness {
            get => lineThickness;
            set => SetProperty(ref lineThickness, value);
        }

        public override IComponentMetadata GetMetadata() => new AzureKinectBodyTrackerVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AzureKinectBodyTrackerVisualizer(pipeline) {
            Mute = Mute,
            LineThickness = LineThickness,
            CircleRadius = CircleRadius,
        };
    }
}
