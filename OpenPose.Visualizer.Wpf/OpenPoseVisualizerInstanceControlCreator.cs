using System.Composition;
using System.Windows;
using OpenSense.Component.OpenPose.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.OpenPose.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenPoseVisualizer;

        public UIElement Create(object instance) => new OpenPoseVisualizerInstanceControl() { DataContext = instance };
    }
}
