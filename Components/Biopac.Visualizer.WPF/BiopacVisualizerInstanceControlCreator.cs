using System.Composition;
using System.Windows;
using OpenSense.Components.Biopac.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Biopac.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class BiopacVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is BiopacVisualizer;

        public UIElement Create(object instance) => new BiopacVisualizerInstanceControl() { DataContext = instance };
    }
}
