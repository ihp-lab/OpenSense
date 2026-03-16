using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class FileWriterInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is FileWriter;

        public UIElement Create(object instance) => new FileWriterInstanceControl() {
            DataContext = instance,
        };
    }
}
