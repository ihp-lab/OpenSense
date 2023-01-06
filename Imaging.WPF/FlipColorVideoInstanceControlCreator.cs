using System.Composition;
using System.Windows;
using OpenSense.Component.Imaging;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Imaging {
    [Export(typeof(IInstanceControlCreator))]
    public class FlipColorVideoInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FlipColorVideo;

        public UIElement Create(object instance) => new FlipColorVideoInstanceControl() { DataContext = instance };
    }
}
