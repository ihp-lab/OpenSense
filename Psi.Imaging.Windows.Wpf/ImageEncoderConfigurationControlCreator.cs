using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.Imaging;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class ImageEncoderConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ImageEncoderConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ImageEncoderConfigurationControl() { DataContext = configuration };
    }
}
