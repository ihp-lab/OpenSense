using System.Composition;
using System.Windows;
using OpenSense.Component.Biopac.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Biopac.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class BiopacVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BiopacVisualizer;

        public UIElement Create(object instance) => new BiopacVisualizerInstanceControl() { DataContext = instance };
    }
}
