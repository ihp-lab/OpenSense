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

        private bool drawPose = true;

        public bool DrawPose {
            get => drawPose;
            set => SetProperty(ref drawPose, value);
        }

        private bool drawFace = true;

        public bool DrawFace {
            get => drawFace;
            set => SetProperty(ref drawFace, value);
        }

        private bool drawHand = true;

        public bool DrawHand {
            get => drawHand;
            set => SetProperty(ref drawHand, value);
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

        public override IComponentMetadata GetMetadata() => new OpenPoseVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenPoseVisualizer(pipeline) {
            Mute = Mute,
            DrawPose = DrawPose,
            DrawFace = DrawFace,
            DrawHand = DrawHand,
            LineThickness = LineThickness,
            CircleRadius = CircleRadius,
        };
    }
}
