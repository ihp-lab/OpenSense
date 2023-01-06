using System.Composition;
using System.Windows;
using OpenSense.Component.Imaging.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Imaging.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class ColorVideoVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ColorVideoVisualizer;

        public UIElement Create(object instance) => new ColorVideoVisualizerInstanceControl() { DataContext = instance };
    }
}
