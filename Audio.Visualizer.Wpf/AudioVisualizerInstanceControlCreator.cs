using System.Composition;
using System.Windows;
using OpenSense.Component.Audio.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Audio.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class AudioVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is AudioVisualizer;

        public UIElement Create(object instance) => new AudioVisualizerInstanceControl() { DataContext = instance };
    }
}
