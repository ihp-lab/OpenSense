using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Component.EyePointOfInterest.Visualizer {
    public class DisplayPoiVisualizer : INotifyPropertyChanged, IConsumer<Vector2> {

        public Receiver<Vector2> In { get; private set; }

        public DisplayPoiVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Vector2>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(Vector2 displayCoordinate, Envelope envelope) {
            X = displayCoordinate.X;
            Y = displayCoordinate.Y;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            X = null;
            Y = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private float? x = null;

        public float? X {
            get => x;
            private set => SetProperty(ref x, value);
        }
        private float? y = null;

        public float? Y {
            get => y;
            private set => SetProperty(ref y, value);
        }
    }
}
