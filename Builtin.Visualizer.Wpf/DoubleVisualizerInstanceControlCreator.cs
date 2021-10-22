using System.Composition;
using System.Windows;
using OpenSense.Component.Builtin.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class DoubleVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DoubleVisualizer;

        public UIElement Create(object instance) => new DoubleVisualizerInstanceControl() { DataContext = instance };
    }
}
