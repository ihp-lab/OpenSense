using System.Composition;
using System.Windows;
using OpenSense.Components.Imaging.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class ImageVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ImageVisualizer;

        public UIElement Create(object instance) => new ImageVisualizerInstanceControl() { DataContext = instance };
    }
}
