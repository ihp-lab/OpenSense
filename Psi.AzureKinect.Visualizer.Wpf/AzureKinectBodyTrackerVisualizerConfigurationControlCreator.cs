using System.Composition;
using System.Windows;
using OpenSense.Component.Psi.AzureKinect.Visualizer;
using OpenSense.Component.Contract;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.AzureKinect.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerVisualizerConfigurationControl() { DataContext = configuration };
    }
}
