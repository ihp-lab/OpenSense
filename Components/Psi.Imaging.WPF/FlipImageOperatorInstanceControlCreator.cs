using System.Composition;
using System.Windows;
using OpenSense.Components.Psi.Imaging;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.Imaging {
    [Export(typeof(IInstanceControlCreator))]
    public class FlipImageOperatorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FlipImageOperator;

        public UIElement Create(object instance) => new FlipImageOperatorInstanceControl() { DataContext = instance };
    }
}
