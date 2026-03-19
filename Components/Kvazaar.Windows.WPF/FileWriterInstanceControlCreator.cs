using System.Composition;
using System.Windows;
using OpenSense.Components.Kvazaar;

namespace OpenSense.WPF.Components.Kvazaar {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class FileWriterInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FileWriter;

        public UIElement Create(object instance) => new FileWriterInstanceControl() {
            DataContext = instance,
        };
    }
}
