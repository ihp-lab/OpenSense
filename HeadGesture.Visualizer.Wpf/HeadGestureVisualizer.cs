using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using HeadGestureData = OpenSense.Component.Head.Common.HeadGesture;

namespace OpenSense.Component.HeadGesture.Visualizer {
    public class HeadGestureVisualizer : IConsumer<HeadGestureData>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<HeadGestureData> In { get; private set; }

        public HeadGestureVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<HeadGestureData>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(HeadGestureData value, Envelope envelope) {
            Value = value;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Value = null;
        }

        private HeadGestureData? val;

        public HeadGestureData? Value {
            get => val;
            set => SetProperty(ref val, value);
        }
    }
}
