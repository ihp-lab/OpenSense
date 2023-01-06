using System.Composition;
using System.Windows;
using OpenSense.Components.Audio.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Audio.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class AudioVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is AudioVisualizer;

        public UIElement Create(object instance) => new AudioVisualizerInstanceControl() { DataContext = instance };
    }
}
