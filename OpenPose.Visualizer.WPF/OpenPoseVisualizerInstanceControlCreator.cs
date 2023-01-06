using System.Composition;
using System.Windows;
using OpenSense.Component.OpenPose.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.OpenPose.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenPoseVisualizer;

        public UIElement Create(object instance) => new OpenPoseVisualizerInstanceControl() { DataContext = instance };
    }
}
