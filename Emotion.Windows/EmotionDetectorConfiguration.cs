using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Emotion {
    [Serializable]
    public class EmotionDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new EmotionDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new EmotionDetector(pipeline) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerProvider)) as ILoggerProvider)?.CreateLogger(Name),
        };
    }
}
