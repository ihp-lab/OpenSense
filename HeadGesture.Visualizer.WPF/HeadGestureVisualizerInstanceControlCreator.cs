using System.Composition;
using System.Windows;
using OpenSense.Components.HeadGesture.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.HeadGesture.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class HeadGestureVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is HeadGestureVisualizer;

        public UIElement Create(object instance) => new HeadGestureVisualizerInstanceControl() { DataContext = instance };
    }
}
