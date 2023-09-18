using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.GoogleCloud.Speech.V1 {
    [Serializable]
    public class GoogleCloudSpeechConfiguration : ConventionalComponentConfiguration {

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        private bool atMostOneFinalResultEachVadSession = false;

        public bool AtMostOneFinalResultEachVadSession {
            get => atMostOneFinalResultEachVadSession;
            set => SetProperty(ref atMostOneFinalResultEachVadSession, value);
        }

        private CredentialSource credentialSource = CredentialSource.Embedded;

        public CredentialSource CredentialSource {
            get => credentialSource;
            set => SetProperty(ref credentialSource, value);
        }

        private string credentials = "Paste your google cloud crednetials JSON content here.";

        public string Credentials {
            get => credentials;
            set => SetProperty(ref credentials, value);
        }

        private string credentialsPath = "set_your_google_cloud_credentials.json";

        public string CredentialsPath {
            get => credentialsPath;
            set => SetProperty(ref credentialsPath, value);
        }

        private string languateCode = "en-US";

        public string LanguageCode {
            get => languateCode;
            set => SetProperty(ref languateCode, value);
        }

        private bool separateRecognitionPerChannel = false;

        /// <summary>
        /// If not set to true, only the first channel is recognized.
        /// </summary>
        public bool SeparateRecognitionPerChannel {
            get => separateRecognitionPerChannel;
            set => SetProperty(ref separateRecognitionPerChannel, value);
        }

        private bool postInterimResults = true;

        public bool PostInterimResults {
            get => postInterimResults;
            set => SetProperty(ref postInterimResults, value);
        }

        private bool addDurationToOutputTime = false;

        public bool AddDurationToOutputTime {
            get => addDurationToOutputTime;
            set => SetProperty(ref addDurationToOutputTime, value);
        }

        public override IComponentMetadata GetMetadata() => new GoogleCloudSpeechMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) {
            var content = CredentialSource switch { 
                CredentialSource.Embedded => Credentials,
                CredentialSource.File => File.ReadAllText(CredentialsPath),
                _ => throw new InvalidOperationException("Invalid Enum value."),
            };
            var result = new GoogleCloudSpeech(pipeline, content) {
                Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
                Mute = Mute,
                AtMostOneFinalResultEachVadSession = AtMostOneFinalResultEachVadSession,
                LanguageCode = LanguageCode,
                SeparateRecognitionPerChannel = SeparateRecognitionPerChannel,
                PostInterimResults = PostInterimResults,
                AddDurationToOutputTime = AddDurationToOutputTime,
            };
            return result;
        }
    }
}
