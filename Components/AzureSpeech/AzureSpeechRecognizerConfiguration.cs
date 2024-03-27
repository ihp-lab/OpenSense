using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.Audio;

namespace OpenSense.Components.AzureSpeech {
    [Serializable]
    public sealed class AzureSpeechRecognizerConfiguration : ConventionalComponentConfiguration {
        #region Settings
        private string key = "<key>";

        public string Key {
            get => key;
            set => SetProperty(ref key, value.Trim());
        }
        
        private string region = "<region>";

        public string Region {
            get => region;
            set => SetProperty(ref region, value.Trim());
        }

        private string language = "en-US";

        public string Language {
            get => language;
            set => SetProperty(ref language, value.Trim());
        }

        private ProfanityOption profanity = ProfanityOption.Raw;

        public ProfanityOption Profanity {
            get => profanity;
            set => SetProperty(ref profanity, value);
        }

        private OutputFormat mode = OutputFormat.Detailed;

        public OutputFormat Mode {
            get => mode;
            set => SetProperty(ref mode, value);
        }

        private TimestampMode inputTimestampMode = TimestampMode.AtEnd;//\psi convention

        public TimestampMode InputTimestampMode {
            get => inputTimestampMode;
            set => SetProperty(ref inputTimestampMode, value);
        }

        private TimeSpan durationThreshold = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// This is a threshold of the duration of the not-yet-recognized-as-final audio.
        /// If the duration of the not-yet-recognized-as-final audio is less than this threshold, the recognizer will post a final result immediately thus not need to wait for timeout.
        /// This value should be high, but still less than the duration of any speech that may contain meaningful content.
        /// </summary>
        public TimeSpan DurationThreshold {
            get => durationThreshold;
            set => SetProperty(ref durationThreshold, value);
        }

        public double DurationThresholdInSeconds {
            get => DurationThreshold.TotalSeconds;
            set => DurationThreshold = TimeSpan.FromSeconds(value);
        }

        private TimeSpan resultTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// This is the timeout for potential waiting in-the-air final results before posting a final result for each voice activity session.
        /// </summary>
        public TimeSpan ResultTimeout {
            get => resultTimeout;
            set => SetProperty(ref resultTimeout, value);
        }

        public double ResultTimeoutInSeconds {
            get => ResultTimeout.TotalSeconds;
            set => ResultTimeout = TimeSpan.FromSeconds(value);
        }

        private string joinSeparator = " ";

        public string JoinSeparator {
            get => joinSeparator;
            set => SetProperty(ref joinSeparator, value);
        }

        private bool postEmptyResults;

        public bool PostEmptyResults {
            get => postEmptyResults;
            set => SetProperty(ref postEmptyResults, value);
        }

        private bool outputAudio;

        public bool OutputAudio {
            get => outputAudio;
            set => SetProperty(ref outputAudio, value);
        }

        private TimestampMode outputTimestampMode = TimestampMode.AtEnd;

        public TimestampMode OutputTimestampMode {
            get => outputTimestampMode;
            set => SetProperty(ref outputTimestampMode, value);
        }
        #endregion

        #region ConventionalComponentConfiguration
        public override IComponentMetadata GetMetadata() => new AzureSpeechRecognizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider? serviceProvider) => new AzureSpeechRecognizer(pipeline, this) {
            Logger = serviceProvider?.GetService(typeof(ILogger)) as ILogger,
        };
        #endregion
    }
}
