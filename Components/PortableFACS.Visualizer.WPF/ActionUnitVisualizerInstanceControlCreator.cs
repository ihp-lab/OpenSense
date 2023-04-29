using System.Composition;
using System.Windows;
using OpenSense.Components.PortableFACS.Visualizer;
using OpenSense.WPF.Components.Contract;
using OpenSense.WPF.Components.PortableFACS;

namespace OpenSense.WPF.Components.Builtin.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class ActionUnitVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ActionUnitVisualizer;

        public UIElement Create(object instance) => new ActionUnitVisualizerInstanceControl() { DataContext = instance };
    }
}
