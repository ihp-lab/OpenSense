using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Audio;

namespace OpenSense.Components.CognitiveServices.Speech {
    public sealed class AudioEnhancementProcessor : IConsumer<AudioBuffer>, IProducer<AudioBuffer>, INotifyPropertyChanged, IDisposable {

        private readonly Lazy<SpeechConfig> _speechConfig;

        private readonly Lazy<AudioProcessingOptions> _processingOptions;

        private readonly Lazy<MemoryAudioStream> _inputStream;

        #region Options
        private string subscriptionKey = "";

        public string SubscriptionKey {
            get => subscriptionKey;
            set => SetProperty(ref subscriptionKey, value);
        }

        private string region = "";

        public string Region {
            get => region;
            set => SetProperty(ref region, value);
        } 

        private ILogger? logger = null;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        #region Ports
        public Receiver<AudioBuffer> In { get; }

        public Emitter<AudioBuffer> Out { get; }
        #endregion

        public AudioEnhancementProcessor(Pipeline pipeline) {
            In = pipeline.CreateReceiver<AudioBuffer>(this, Process, nameof(In));
            Out = pipeline.CreateEmitter<AudioBuffer>(this, nameof(Out));

            _speechConfig = new Lazy<SpeechConfig>(LazyCreateSpeechConfig);
            _processingOptions = new Lazy<AudioProcessingOptions>(LazyCreateProcessingOptions);
            _inputStream = new Lazy<MemoryAudioStream>(LazyCreateInputStream);
        }

        #region Lazy Initialize
        private SpeechConfig LazyCreateSpeechConfig() {
            var result = SpeechConfig.FromSubscription(SubscriptionKey, Region);
            return result;
        }

        private AudioProcessingOptions LazyCreateProcessingOptions() {
            throw new NotImplementedException();
        } 

        private MemoryAudioStream LazyCreateInputStream() {
            throw new NotImplementedException();
        }
        #endregion

        private void Process(AudioBuffer data, Envelope envelope) {
            throw new NotImplementedException();
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }
            disposed = true;

            if (_processingOptions.IsValueCreated) {
                _processingOptions.Value.Dispose();
            }
            if (_inputStream.IsValueCreated) {
                _inputStream.Value.Dispose();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
