using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.LibreFace {
    [Export(typeof(IInstanceControlCreator))]
    public class ActionUnitPresenceVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ActionUnitPresenceVisualizer;

        public UIElement Create(object instance) => new ActionUnitPresenceVisualizerInstanceControl() { DataContext = instance };
    }
}
