using System.Composition;
using System.Windows;
using OpenSense.Component.Emotion.Visualizer;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Emotion.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class EmotionVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is EmotionVisualizer;

        public UIElement Create(object instance) => new EmotionVisualizerInstanceControl() { DataContext = instance };
    }
}
