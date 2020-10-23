using Microsoft.Psi;
using Microsoft.Psi.Audio;
using OpenSense.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenSense.Components {
    public class VadComponent : IConsumer<bool>, INotifyPropertyChanged {

        public Receiver<bool> In { get; private set; }

        public VadComponent(Pipeline pipeline) {
            In = pipeline.CreateReceiver<bool>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(bool vad, Envelope envelope) {
            Detected = vad;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Detected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private bool detected;

        public bool Detected {
            get => detected;
            set => SetProperty(ref detected, value);
        }
    }
}
