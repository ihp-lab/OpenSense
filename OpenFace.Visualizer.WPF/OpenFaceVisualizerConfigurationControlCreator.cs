using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.OpenFace.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.OpenFace.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenFaceVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenFaceVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenFaceVisualizerConfigurationControl() { DataContext = configuration };
    }
}
