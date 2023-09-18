using System.Composition;
using System.Windows;
using OpenSense.Components;

namespace OpenSense.WPF.Components.MediaPipe.NET.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class NormalizedLandmarkListVectorVisualizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is NormalizedLandmarkListVectorVisualizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new NormalizedLandmarkListVectorVisualizerConfigurationControl() { DataContext = configuration };
    }
}
