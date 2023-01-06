using System.Composition;
using System.Windows;
using OpenSense.Components.AzureKinect.Visualizer;
using OpenSense.Components.Contract;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.AzureKinect.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerVisualizerConfigurationControl() { DataContext = configuration };
    }
}
