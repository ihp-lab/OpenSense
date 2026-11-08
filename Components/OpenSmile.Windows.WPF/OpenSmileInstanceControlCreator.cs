using System.Composition;
using System.Windows;

namespace OpenSense.WPF.Components.OpenSmile;

[Export(typeof(IInstanceControlCreator))]
public sealed class OpenSmileInstanceControlCreator : IInstanceControlCreator {

    public bool CanCreate(object instance) => instance is OpenSense.Components.OpenSmile.OpenSmile;

    public UIElement Create(object instance) => new OpenSmileInstanceControl() {
        DataContext = instance,
    };
}
