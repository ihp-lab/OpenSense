using System.Composition;
using System.Windows;
using OpenSense.Components.Builtin.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class BooleanVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BooleanVisualizer;

        public UIElement Create(object instance) => new BooleanVisualizerInstanceControl() { DataContext = instance };
    }
}
