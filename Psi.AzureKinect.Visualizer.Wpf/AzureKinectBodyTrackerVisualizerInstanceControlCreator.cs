using System.Composition;
using System.Windows;
using OpenSense.Component.Psi.AzureKinect.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.AzureKinect.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class AzureKinectBodyTrackerVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is AzureKinectBodyTrackerVisualizer;

        public UIElement Create(object instance) => new AzureKinectBodyTrackerVisualizerInstanceControl() { DataContext = instance };
    }
}
