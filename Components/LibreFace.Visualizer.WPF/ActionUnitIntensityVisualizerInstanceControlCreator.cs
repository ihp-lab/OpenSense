using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class ActionUnitIntensityVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is ActionUnitIntensityVisualizer;

        public UIElement Create(object instance) => new ActionUnitIntensityVisualizerInstanceControl() { DataContext = instance };
    }
}
