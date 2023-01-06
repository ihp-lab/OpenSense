using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Microsoft.Psi.Speech;

namespace OpenSense.Component.Psi.Speech.Visualizer {
    public sealed class StreamingSpeechRecognitionVisualizer : IConsumer<IStreamingSpeechRecognitionResult>, INotifyPropertyChanged {

        #region Settings
        private bool onlyUpdateOnFinalResults = false;

        public bool OnlyUpdateOnFinalResults {
            get => onlyUpdateOnFinalResults;
            set => SetProperty(ref onlyUpdateOnFinalResults, value);
        }
        #endregion

        public Receiver<IStreamingSpeechRecognitionResult> In { get; private set; }

        public StreamingSpeechRecognitionVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<IStreamingSpeechRecognitionResult>(this, Porcess, nameof(In));
            pipeline.PipelineCompleted += PipelineCompleted;
        }

        private void Porcess(IStreamingSpeechRecognitionResult speech, Envelope envelope) {
            if (OnlyUpdateOnFinalResults && !speech.IsFinal) {
                return;
            }
            Timestamp = envelope.OriginatingTime;
            Result = speech;
        }

        private void PipelineCompleted(object sender, PipelineCompletedEventArgs e) {
            Result = null;
            Timestamp = null;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        } 
        #endregion

        private DateTime? timestamp = null;

        public DateTime? Timestamp {
            get => timestamp;
            private set => SetProperty(ref timestamp, value);
        }

        private IStreamingSpeechRecognitionResult result = null;

        public IStreamingSpeechRecognitionResult Result {
            get => result;
            private set => SetProperty(ref result, value);
        }
        
    }
}
