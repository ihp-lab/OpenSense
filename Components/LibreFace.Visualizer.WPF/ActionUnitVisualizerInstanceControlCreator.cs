using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.LibreFace {
    [Export(typeof(IInstanceControlCreator))]
    public class ActionUnitVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ActionUnitVisualizer;

        public UIElement Create(object instance) => new ActionUnitVisualizerInstanceControl() { DataContext = instance };
    }
}
