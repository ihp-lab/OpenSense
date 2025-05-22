using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.AzureKinect.BodyTracking.Visualizer;

namespace OpenSense.WPF.Components.AzureKinect.BodyTracking.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class AzureKinectBodyTrackerVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerVisualizerConfigurationControl() { DataContext = configuration };
    }
}
