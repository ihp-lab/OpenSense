using System.Composition;
using System.Windows;
using OpenSense.Components.Psi.Speech.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.Speech.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class StreamingSpeechRecognitionVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is StreamingSpeechRecognitionVisualizer;

        public UIElement Create(object instance) => new StreamingSpeechRecognitionVisualizerInstanceControl() { DataContext = instance };
    }
}
