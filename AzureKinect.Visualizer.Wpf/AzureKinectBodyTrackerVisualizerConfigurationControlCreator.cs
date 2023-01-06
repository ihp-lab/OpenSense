using System.Composition;
using System.Windows;
using OpenSense.Component.AzureKinect.Visualizer;
using OpenSense.Component.Contract;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.AzureKinect.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerVisualizerConfigurationControl() { DataContext = configuration };
    }
}
