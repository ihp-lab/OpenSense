using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.OpenPose.PInvoke.Configuration;

namespace OpenSense.Components.OpenPose {
    [Serializable]
    public class OpenPoseConfiguration : ConventionalComponentConfiguration {

        private AggregateStaticConfiguration raw = new AggregateStaticConfiguration();

        public AggregateStaticConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenPoseMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenPose(pipeline, Raw) { 
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
