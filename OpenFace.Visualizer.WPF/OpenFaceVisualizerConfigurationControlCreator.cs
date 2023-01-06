using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.OpenFace.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.OpenFace.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenFaceVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenFaceVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenFaceVisualizerConfigurationControl() { DataContext = configuration };
    }
}
