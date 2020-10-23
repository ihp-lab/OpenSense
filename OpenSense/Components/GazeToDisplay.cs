using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using OpenSense.DataStructure;
using OpenSense.GazeToDisplayConverter;

namespace OpenSense.Components {
    public class GazeToDisplay : IConsumerProducer<HeadPoseAndGaze, ValueTuple<double, double>> {

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Receiver<HeadPoseAndGaze> In { get; private set; }

        public Emitter<(double, double)> Out { get; private set; }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private IGazeToDisplayConverter converter;

        public IGazeToDisplayConverter Converter {
            get => converter;
            set => SetProperty(ref converter, value);
        }

        public GazeToDisplay(Pipeline pipeline) {
            In = pipeline.CreateReceiver<HeadPoseAndGaze>(this, Porcess, nameof(In));
            Out = pipeline.CreateEmitter<ValueTuple<double, double>>(this, nameof(Out));
        }

        private void Porcess(HeadPoseAndGaze headPoseAndGaze, Envelope envelope) {
            if (Mute) {
                return;
            }
            var cvt = Converter;
            if (cvt is null) {
                return;
            }
            var result = cvt.Predict(headPoseAndGaze);
            Out.Post(result, envelope.OriginatingTime);
        }
    }
}
