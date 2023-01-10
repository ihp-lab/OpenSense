using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.OpenSmile.Visualizer {
    public class OpenSmileVisualizer : IConsumer<Vector<float>>, INotifyPropertyChanged {

        public Receiver<Vector<float>> In { get; private set; }

        public OpenSmileVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Vector<float>>(this, Receive, nameof(In));
            pipeline.PipelineCompleted += (sender, e) => Vector = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            //if (!EqualityComparer<T>.Default.Equals(field, value)) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            //}
        }

        private Vector<float> vector;

        public Vector<float> Vector {
            get => vector;
            private set => SetProperty(ref vector, value);
        }

        private void Receive(Vector<float> input, Envelope envelope) {
            Vector = input.DeepClone();
        }
    }
}
