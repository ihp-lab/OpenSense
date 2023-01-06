using System.Composition;
using System.Windows;
using OpenSense.Components.Imaging.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class ColorVideoVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ColorVideoVisualizer;

        public UIElement Create(object instance) => new ColorVideoVisualizerInstanceControl() { DataContext = instance };
    }
}
