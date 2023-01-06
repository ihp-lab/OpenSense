using System.Composition;
using System.Windows;
using OpenSense.Component.Builtin.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class BooleanVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BooleanVisualizer;

        public UIElement Create(object instance) => new BooleanVisualizerInstanceControl() { DataContext = instance };
    }
}
