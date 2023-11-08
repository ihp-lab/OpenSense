using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;

namespace OpenSense.WPF.Components.LibreFace.Visualizer {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class FacialExpressionVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FacialExpressionVisualizer;

        public UIElement Create(object instance) => new FacialExpressionVisualizerInstanceControl() { DataContext = instance };
    }
}
