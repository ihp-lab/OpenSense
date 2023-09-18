using System.Composition;
using System.Windows;
using OpenSense.Components.EyePointOfInterest.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.EyePointOfInterest.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class DisplayPoiVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DisplayPoiVisualizer;

        public UIElement Create(object instance) => new DisplayPoiVisualizerInstanceControl() { DataContext = instance };
    }
}
