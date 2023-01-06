using System.Composition;
using System.Windows;
using OpenSense.Components.Builtin.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class DoubleVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DoubleVisualizer;

        public UIElement Create(object instance) => new DoubleVisualizerInstanceControl() { DataContext = instance };
    }
}
