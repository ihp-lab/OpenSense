using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.AzureKinect.Visualizer;

namespace OpenSense.WPF.Components.AzureKinect.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerVisualizerConfigurationControl() { DataContext = configuration };
    }
}
