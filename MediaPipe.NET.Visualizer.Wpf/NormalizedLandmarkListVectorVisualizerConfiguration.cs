using System;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.WPF.Component.MediaPipe.NET.Visualizer {
    [Serializable]
    public sealed class NormalizedLandmarkListVectorVisualizerConfiguration : ConventionalComponentConfiguration {
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

        private bool drawLandmarks = true;

        public bool DrawLandmarks {
            get => drawLandmarks;
            set => SetProperty(ref drawLandmarks, value);
        }

        private bool drawLips = false;

        public bool DrawLips {
            get => drawLips;
            set => SetProperty(ref drawLips, value);
        }

        private bool drawLeftEye = false;

        public bool DrawLeftEye {
            get => drawLeftEye;
            set => SetProperty(ref drawLeftEye, value);
        }

        private bool drawRightEye = false;

        public bool DrawRightEye {
            get => drawRightEye;
            set => SetProperty(ref drawRightEye, value);
        }

        private bool drawLeftEyebrow = false;

        public bool DrawLeftEyebrow {
            get => drawLeftEyebrow;
            set => SetProperty(ref drawLeftEyebrow, value);
        }

        private bool drawRightEyebrow = false;

        public bool DrawRightEyebrow {
            get => drawRightEyebrow;
            set => SetProperty(ref drawRightEyebrow, value);
        }

        private bool drawLeftIris = false;

        public bool DrawLeftIris {
            get => drawLeftIris;
            set => SetProperty(ref drawLeftIris, value);
        }

        private bool drawRightIris = false;

        public bool DrawRightIris {
            get => drawRightIris;
            set => SetProperty(ref drawRightIris, value);
        }

        private bool drawFaceOval = false;

        public bool DrawFaceOval {
            get => drawFaceOval;
            set => SetProperty(ref drawFaceOval, value);
        }

        private bool drawTesselation = false;

        public bool DrawTesselation {
            get => drawTesselation;
            set => SetProperty(ref drawTesselation, value);
        }

        public override IComponentMetadata GetMetadata() => new NormalizedLandmarkListVectorVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new NormalizedLandmarkListVectorVisualizer(pipeline) {
            Mute = Mute,
            LineThickness = LineThickness,
            CircleRadius = CircleRadius,
            DrawLandmarks = DrawLandmarks,
            DrawLips = DrawLips,
            DrawLeftEye = DrawLeftEye,
            DrawRightEye = DrawRightEye,
            DrawLeftEyebrow = DrawLeftEyebrow,
            DrawRightEyebrow = DrawRightEyebrow,
            DrawLeftIris = DrawLeftIris,
            DrawRightIris = DrawRightIris,
            DrawFaceOval = DrawFaceOval,
            DrawTesselation = DrawTesselation,
        };
    }
}
