using System.Composition;
using System.Windows;
using OpenSense.Components.OpenPose.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.OpenPose.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenPoseVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenPoseVisualizer;

        public UIElement Create(object instance) => new OpenPoseVisualizerInstanceControl() { DataContext = instance };
    }
}
