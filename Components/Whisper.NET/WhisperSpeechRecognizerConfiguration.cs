using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Whisper.net.Ggml;

namespace OpenSense.Components.Whisper.NET {
    [Serializable]
    public sealed class WhisperSpeechRecognizerConfiguration : ConventionalComponentConfiguration {

        #region Options
        private string modelDirectory = "";

        public string ModelDirectory {
            get => modelDirectory;
            set => SetProperty(ref modelDirectory, value);
        }

        private GgmlType modelType = GgmlType.BaseEn;

        public GgmlType ModelType {
            get => modelType;
            set => SetProperty(ref modelType, value);
        }

        private QuantizationType quantizationType = QuantizationType.Q5_1;

        public QuantizationType QuantizationType {
            get => quantizationType;
            set => SetProperty(ref quantizationType, value);
        }

        private bool forceDownload = false;

        public bool ForceDownload {
            get => forceDownload;
            set => SetProperty(ref forceDownload, value);
        }

        private double downloadTimeoutInSeconds = 15;

        public double DownloadTimeoutInSeconds {
            get => downloadTimeoutInSeconds;
            set => SetProperty(ref downloadTimeoutInSeconds, value);
        }

        private bool lazyInitialization = false;

        public bool LazyInitialization {
            get => lazyInitialization;
            set => SetProperty(ref lazyInitialization, value);
        }

        private Language language = Language.English;

        public Language Language {
            get => language;
            set => SetProperty(ref language, value);
        }

        private string prompt = "";

        public string Prompt {
            get => prompt;
            set => SetProperty(ref prompt, value);
        }

        private SegmentationRestriction segmentationRestriction = SegmentationRestriction.OnePerUtterence;

        public SegmentationRestriction SegmentationRestriction {
            get => segmentationRestriction;
            set => SetProperty(ref segmentationRestriction, value);
        }

        private TimestampMode inputTimestampMode = TimestampMode.AtEnd;//\psi convention

        public TimestampMode InputTimestampMode {
            get => inputTimestampMode;
            set => SetProperty(ref inputTimestampMode, value);
        }

        private TimestampMode outputTimestampMode = TimestampMode.AtEnd;

        public TimestampMode OutputTimestampMode {
            get => outputTimestampMode;
            set => SetProperty(ref outputTimestampMode, value);
        }

        private bool outputPartialResults = false;

        public bool OutputPartialResults {
            get => outputPartialResults;
            set => SetProperty(ref outputPartialResults, value);
        }

        private double partialEvalueationInvervalInSeconds = 0.5;

        public double PartialEvalueationInvervalInSeconds {
            get => partialEvalueationInvervalInSeconds;
            set => SetProperty(ref partialEvalueationInvervalInSeconds, value);
        }

        private bool outputAudio = false;

        public bool OutputAudio {
            get => outputAudio;
            set => SetProperty(ref outputAudio, value);
        }
        #endregion

        public override IComponentMetadata GetMetadata() => new WhisperSpeechRecognizerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new WhisperSpeechRecognizer(pipeline) {
            ModelDirectory = ModelDirectory,
            ModelType = ModelType,
            QuantizationType = QuantizationType,
            ForceDownload = ForceDownload,
            DownloadTimeout = TimeSpan.FromSeconds(DownloadTimeoutInSeconds),
            LazyInitialization = LazyInitialization,
            Language = Language,
            Prompt = Prompt,
            SegmentationRestriction = SegmentationRestriction,
            InputTimestampMode = InputTimestampMode,
            OutputTimestampMode = OutputTimestampMode,
            OutputPartialResults = OutputPartialResults,
            PartialEvalueationInverval = TimeSpan.FromSeconds(PartialEvalueationInvervalInSeconds),
            OutputAudio = OutputAudio,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
