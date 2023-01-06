using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using OpenSense.Components.EyePointOfInterest.Common;
using OpenSense.Components.OpenFace.Common;

namespace OpenSense.Components.EyePointOfInterest {
    public class DisplayPoiEstimator : IConsumerProducer<PoseAndEyeAndFace, Vector2> {

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Receiver<PoseAndEyeAndFace> In { get; private set; }

        public Emitter<Vector2> Out { get; private set; }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private IPoiOnDisplayEstimator estimator;

        public IPoiOnDisplayEstimator Estimator {
            get => estimator;
            set => SetProperty(ref estimator, value);
        }

        public DisplayPoiEstimator(Pipeline pipeline) {
            In = pipeline.CreateReceiver<PoseAndEyeAndFace>(this, Porcess, nameof(In));
            Out = pipeline.CreateEmitter<Vector2>(this, nameof(Out));
        }

        private void Porcess(PoseAndEyeAndFace headPoseAndGaze, Envelope envelope) {
            if (Mute) {
                return;
            }
            var cvt = Estimator;
            if (cvt is null) {
                return;
            }
            var result = cvt.Predict(headPoseAndGaze);
            Out.Post(result, envelope.OriginatingTime);
        }
    }
}
