using System.Composition;
using System.Windows;
using OpenSense.Components.AzureKinect.BodyTracking.Visualizer;

namespace OpenSense.WPF.Components.AzureKinect.BodyTracking.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class AzureKinectBodyTrackerVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is AzureKinectBodyTrackerVisualizer;

        public UIElement Create(object instance) => new AzureKinectBodyTrackerVisualizerInstanceControl() { DataContext = instance };
    }
}
