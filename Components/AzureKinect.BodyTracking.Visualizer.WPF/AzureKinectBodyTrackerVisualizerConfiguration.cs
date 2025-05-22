using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.AzureKinect.BodyTracking.Visualizer {
    [Serializable]
    public sealed class AzureKinectBodyTrackerVisualizerConfiguration : ConventionalComponentConfiguration {

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
            Logger = serviceProvider.GetService(typeof(ILogger<AzureKinectBodyTrackerVisualizer>)) as ILogger,
        };
    }
}
