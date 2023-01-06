using System.Composition;
using System.Windows;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.OpenSmile {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenSmileInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Component.OpenSmile.OpenSmile;

        public UIElement Create(object instance) => new OpenSmileInstanceControl() { DataContext = instance };
    }
}
