using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;

namespace OpenSense.Components.Display {
    public class BooleanVisualizer : IConsumer<bool>, INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<bool> In { get; private set; }

        public BooleanVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<bool>(this, Process, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Process(bool value, Envelope envelope) {
            Value = value;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Value = null;
        }

        private bool? val;

        public bool? Value {
            get => val;
            set => SetProperty(ref val, value);
        }
    }
}
