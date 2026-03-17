using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class HevcDecoderConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is HevcDecoderConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new HevcDecoderConfigurationControl() {
            DataContext = configuration,
        };
    }
}
