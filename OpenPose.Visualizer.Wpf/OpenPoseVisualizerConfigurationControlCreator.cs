using System.Composition;
using System.Windows;
using OpenSense.Component.OpenPose.Visualizer;
using OpenSense.Component.Contract;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.OpenPose.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenPoseVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenPoseVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenPoseVisualizerConfigurationControl() { DataContext = configuration };
    }
}
