using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using OpenSense.Components.OpenSmile;

namespace OpenSense.Components.Display {
    public class OpenSmileVisualizer : IConsumer<Vector>, INotifyPropertyChanged {

        public Receiver<Vector> In { get; private set; }

        public OpenSmileVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Vector>(this, this.Receive, nameof(In));
            pipeline.PipelineCompleted += (sender, e) => Vector = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            //if (!EqualityComparer<T>.Default.Equals(field, value)) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //}
        }

        private Vector vector;

        public Vector Vector {
            get => vector;
            private set => SetProperty(ref vector, value);
        }

        private void Receive(Vector input, Envelope envelope) {
            Vector = input.DeepClone();
        }
    }
}
