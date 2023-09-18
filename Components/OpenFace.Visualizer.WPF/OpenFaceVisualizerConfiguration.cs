using System;
using Microsoft.Psi;

namespace OpenSense.Components.OpenFace.Visualizer {
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

        private bool drawHeadLandmarks = true;

        public bool DrawHeadLandmarks {
            get => drawHeadLandmarks;
            set => SetProperty(ref drawHeadLandmarks, value);
        }

        private bool drawHeadIndicatorLines = true;

        public bool DrawHeadIndicatorLines {
            get => drawHeadIndicatorLines;
            set => SetProperty(ref drawHeadIndicatorLines, value);
        }

        private bool drawEyeLandmarks = true;

        public bool DrawEyeLandmarks {
            get => drawEyeLandmarks;
            set => SetProperty(ref drawEyeLandmarks, value);
        }

        private bool drawEyeIndicatorLines = true;

        public bool DrawEyeIndicatorLines {
            get => drawEyeIndicatorLines;
            set => SetProperty(ref drawEyeIndicatorLines, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenFaceVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenFaceVisualizer(pipeline) {
            Mute = Mute,
            LineThickness = LineThickness,
            CircleRadius = CircleRadius,
            DrawHeadLandmarks = DrawHeadLandmarks,
            DrawHeadIndicatorLines = DrawHeadIndicatorLines,
            DrawEyeLandmarks = DrawEyeLandmarks,
            DrawEyeIndicatorLines = DrawEyeIndicatorLines,
        };
    }
}
