using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.OpenFace.Visualizer;

namespace OpenSense.WPF.Components.OpenFace.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenFaceVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenFaceVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenFaceVisualizerConfigurationControl() { DataContext = configuration };
    }
}
