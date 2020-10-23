using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Speech;
using OpenSense.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpenSense.Components.Display {
    public class StreamingSpeechRecognitionVisualizer : IConsumer<IStreamingSpeechRecognitionResult>, INotifyPropertyChanged {

        public Receiver<IStreamingSpeechRecognitionResult> In { get; private set; }

        public StreamingSpeechRecognitionVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IStreamingSpeechRecognitionResult>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(IStreamingSpeechRecognitionResult speech, Envelope envelope) {
            Speech = speech.Text;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Speech = string.Empty;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string speech = string.Empty;

        public string Speech {
            get => speech;
            set => SetProperty(ref speech, value);
        }
    }
}
