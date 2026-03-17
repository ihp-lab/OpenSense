using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class HevcEncoderInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is HevcEncoder;

        public UIElement Create(object instance) => new HevcEncoderInstanceControl() {
            DataContext = instance,
        };
    }
}
