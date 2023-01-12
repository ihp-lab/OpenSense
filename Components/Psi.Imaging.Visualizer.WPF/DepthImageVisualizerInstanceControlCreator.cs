using System.Composition;
using System.Windows;
using OpenSense.Components.Psi.Imaging.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Imaging.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class DepthImageVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DepthImageVisualizer;

        public UIElement Create(object instance) => new DepthImageVisualizerInstanceControl() { DataContext = instance };
    }
}
