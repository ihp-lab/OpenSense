using System.Composition;
using System.Windows;
using OpenSense.Components.LibreFace.Visualizer;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.LibreFace {
    [Export(typeof(IInstanceControlCreator))]
    public class FacialExpressionVisualizerInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FacialExpressionVisualizer;

        public UIElement Create(object instance) => new FacialExpressionVisualizerInstanceControl() { DataContext = instance };
    }
}
