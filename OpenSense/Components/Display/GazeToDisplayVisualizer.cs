using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;

namespace OpenSense.Components.Display {
    public class GazeToDisplayVisualizer : INotifyPropertyChanged, IConsumer<ValueTuple<double, double>> {

        public Receiver<ValueTuple<double, double>> In { get; private set; }

        public GazeToDisplayVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<ValueTuple<double, double>>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(ValueTuple<double, double> displayCoordinate, Envelope envelope) {
            X = displayCoordinate.Item1;
            Y = displayCoordinate.Item2;
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

        private double? x = null;

        public double? X {
            get => x;
            private set => SetProperty(ref x, value);
        }
        private double? y = null;

        public double? Y {
            get => y;
            private set => SetProperty(ref y, value);
        }

        

    }
}
