using System;
using Microsoft.Psi;
using Microsoft.Psi.AzureKinect;

namespace OpenSense.Components.Psi.AzureKinect {
    [Serializable]
    public class AzureKinectBodyTrackerConfiguration : ConventionalComponentConfiguration {

        private Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration raw = new Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration();

        public Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override IComponentMetadata GetMetadata() => new AzureKinectBodyTrackerMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new AzureKinectBodyTracker(pipeline, Raw);
    }
}
