using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class ActionUnitPresenceVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ActionUnitPresenceVisualizer;

        public UIElement Create(object instance) => new ActionUnitPresenceVisualizerInstanceControl() { DataContext = instance };
    }
}
