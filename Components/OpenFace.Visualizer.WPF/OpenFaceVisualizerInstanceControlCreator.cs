using System.Composition;
using System.Windows;
using OpenSense.Components.OpenFace.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.OpenFace.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenFaceVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenFaceVisualizer;

        public UIElement Create(object instance) => new OpenFaceVisualizerInstanceControl() { DataContext = instance };
    }
}
