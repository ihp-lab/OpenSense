using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.Whisper.NET {
    [Serializable]
    internal class WhisperConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new WhisperMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new WhisperProcessor(pipeline) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
