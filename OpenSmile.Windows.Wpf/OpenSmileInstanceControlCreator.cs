using System.Composition;
using System.Windows;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.OpenSmile {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenSmileInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Component.OpenSmile.OpenSmile;

        public UIElement Create(object instance) => new OpenSmileInstanceControl() { DataContext = instance };
    }
}
