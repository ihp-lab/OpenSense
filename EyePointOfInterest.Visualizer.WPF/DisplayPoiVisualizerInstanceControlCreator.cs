using System.Composition;
using System.Windows;
using OpenSense.Component.EyePointOfInterest.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.EyePointOfInterest.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class DisplayPoiVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is DisplayPoiVisualizer;

        public UIElement Create(object instance) => new DisplayPoiVisualizerInstanceControl() { DataContext = instance };
    }
}
