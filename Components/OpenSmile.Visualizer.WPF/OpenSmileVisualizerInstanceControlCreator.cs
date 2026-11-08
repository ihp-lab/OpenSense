using System.Composition;
using System.Windows;
using OpenSense.Components.OpenSmile.Visualizer;

namespace OpenSense.WPF.Components.OpenSmile.Visualizer;

[Export(typeof(IInstanceControlCreator))]
public sealed class OpenSmileVisualizerInstanceControlCreator : IInstanceControlCreator {

    public bool CanCreate(object instance) => instance is OpenSmileVisualizer;

    public UIElement Create(object instance) => new OpenSmileVisualizerInstanceControl() {
        DataContext = instance,
    };
}
