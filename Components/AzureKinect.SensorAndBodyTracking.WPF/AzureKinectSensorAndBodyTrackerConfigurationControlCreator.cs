using System.Composition;
using OpenSense.Components;
using System.Windows;
using OpenSense.Components.AzureKinect.SensorAndBodyTracking;

namespace OpenSense.WPF.Components.AzureKinect.SensorAndBodyTracking {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class AzureKinectSensorAndBodyTrackereConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectSensorAndBodyTrackerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectSensorAndBodyTrackerConfigurationControl() {
            DataContext = configuration
        };
    }
}
