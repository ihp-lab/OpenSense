using System.Composition;
using System.Windows;
using OpenSense.Components.Emotion.Visualizer;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Emotion.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class EmotionVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is EmotionVisualizer;

        public UIElement Create(object instance) => new EmotionVisualizerInstanceControl() { DataContext = instance };
    }
}
