using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.Imaging;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class ImageEncoderConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ImageEncoderConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ImageEncoderConfigurationControl() { DataContext = configuration };
    }
}
