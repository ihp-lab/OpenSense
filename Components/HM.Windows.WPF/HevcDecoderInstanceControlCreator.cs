using System.Composition;
using System.Windows;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IInstanceControlCreator))]
    public sealed class HevcDecoderInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is HevcDecoder;

        public UIElement Create(object instance) => new HevcDecoderInstanceControl() {
            DataContext = instance,
        };
    }
}
