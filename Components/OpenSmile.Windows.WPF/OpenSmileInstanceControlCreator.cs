using System.Composition;
using System.Windows;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.OpenSmile {
    [Export(typeof(IInstanceControlCreator))]
    public class OpenSmileInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is OpenSense.Components.OpenSmile.OpenSmile;

        public UIElement Create(object instance) => new OpenSmileInstanceControl() { DataContext = instance };
    }
}
