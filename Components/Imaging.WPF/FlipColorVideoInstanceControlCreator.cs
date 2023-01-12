using System.Composition;
using System.Windows;
using OpenSense.Components.Imaging;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging {
    [Export(typeof(IInstanceControlCreator))]
    public class FlipImageInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FlipImage;

        public UIElement Create(object instance) => new FlipImageInstanceControl() { DataContext = instance };
    }
}
