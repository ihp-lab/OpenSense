using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Emotion {
    [Serializable]
    public class EmotionDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new EmotionDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new EmotionDetector(pipeline) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
