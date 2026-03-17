using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class HevcEncoderConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is HevcEncoderConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new HevcEncoderConfigurationControl() {
            DataContext = configuration,
        };
    }
}
