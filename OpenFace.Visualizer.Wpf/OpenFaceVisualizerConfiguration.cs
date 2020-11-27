using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.OpenFace.Visualizer {
    [Serializable]
    public class OpenFaceVisualizerConfiguration : ConventionalComponentConfiguration {

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

        public override IComponentMetadata GetMetadata() => new OpenFaceVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new OpenFaceVisualizer(pipeline) {
            Mute = Mute,
            LineThickness = LineThickness,
            CircleRadius = CircleRadius,
        };
    }
}
