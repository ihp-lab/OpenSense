using System.Composition;
using System.Windows;
using OpenSense.Component.OpenSmile.Visualizer;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.OpenSmile.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenSmileVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSmileVisualizer;

        public UIElement Create(object instance) => new OpenSmileVisualizerInstanceControl() { DataContext = instance };
    }
}
