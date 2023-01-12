using System.Composition;
using System.Windows;
using OpenSense.Components.Imaging;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging {
    [Export(typeof(IInstanceControlCreator))]
    public class FlipImageOperatorInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FlipImageOperator;

        public UIElement Create(object instance) => new FlipImageOperatorInstanceControl() { DataContext = instance };
    }
}
