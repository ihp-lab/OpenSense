using System.Composition;
using System.Windows;
using OpenSense.Components.Psi.Imaging.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Imaging.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class ImageVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ImageVisualizer;

        public UIElement Create(object instance) => new ImageVisualizerInstanceControl() { DataContext = instance };
    }
}
