using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class FileReaderInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FileReader;

        public UIElement Create(object instance) => new FileReaderInstanceControl() {
            DataContext = instance,
        };
    }
}
