using System.Composition;
using System.Windows;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.MediaPipe.NET.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class NormalizedLandmarkListVectorVisualizerInstanceControlCreator : IInstanceControlCreator {
        public bool CanCreate(object instance) => instance is NormalizedLandmarkListVectorVisualizer;

        public UIElement Create(object instance) => new NormalizedLandmarkListVectorVisualizerInstanceControl() { DataContext = instance };
    }
}
