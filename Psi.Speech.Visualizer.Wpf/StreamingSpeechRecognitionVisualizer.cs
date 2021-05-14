using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Speech;

namespace OpenSense.Component.Psi.Speech.Visualizer {
    public class StreamingSpeechRecognitionVisualizer : IConsumer<IStreamingSpeechRecognitionResult>, INotifyPropertyChanged {

        public Receiver<IStreamingSpeechRecognitionResult> In { get; private set; }

        public StreamingSpeechRecognitionVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IStreamingSpeechRecognitionResult>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(IStreamingSpeechRecognitionResult speech, Envelope envelope) {
            Speech = speech.Text;
            Final = speech.IsFinal;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Speech = string.Empty;
            Final = false;
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

        private bool final = false;

        public bool Final {
            get => final;
            set => SetProperty(ref final, value);
        }
    }
}
