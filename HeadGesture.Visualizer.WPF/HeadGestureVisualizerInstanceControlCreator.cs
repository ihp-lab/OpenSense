using System.Composition;
using System.Windows;
using OpenSense.Component.HeadGesture.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.HeadGesture.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class HeadGestureVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is HeadGestureVisualizer;

        public UIElement Create(object instance) => new HeadGestureVisualizerInstanceControl() { DataContext = instance };
    }
}
