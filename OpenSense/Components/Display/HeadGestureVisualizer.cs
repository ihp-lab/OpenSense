using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using OpenSense.DataStructure;

namespace OpenSense.Components.Display {
    public class HeadGestureVisualizer : IConsumer<HeadGesture>, INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<HeadGesture> In { get; private set; }

        public HeadGestureVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<HeadGesture>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(HeadGesture value, Envelope envelope) {
            Value = value;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Value = null;
        }

        private HeadGesture? val;

        public HeadGesture? Value {
            get => val;
            set => SetProperty(ref val, value);
        }
    }
}
