using System.Composition;
using System.Windows;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class StringVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is StringVisualizer;

        public UIElement Create(object instance) => new StringVisualizerInstanceControl() { DataContext = instance, };
    }
}
