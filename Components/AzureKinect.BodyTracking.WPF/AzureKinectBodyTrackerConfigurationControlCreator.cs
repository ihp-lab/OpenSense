using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.AzureKinect.BodyTracking;

namespace OpenSense.WPF.Components.AzureKinect.BodyTracking {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class AzureKinectBodyTrackerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerConfigurationControl() {
            DataContext = configuration
        };
    }
}
