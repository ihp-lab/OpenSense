using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Component.Contract;
using OpenSense.Component.OpenPose.PInvoke.Configuration;

namespace OpenSense.Component.OpenPose {
    [Serializable]
    public class OpenPoseConfiguration : ConventionalComponentConfiguration {

        private AggregateStaticConfiguration raw = new AggregateStaticConfiguration();

        public AggregateStaticConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new OpenPoseMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new OpenPose(pipeline, Raw) { 
            Logger = serviceProvider?.GetService<ILoggerProvider>()?.CreateLogger(Name),
        };
    }
}
