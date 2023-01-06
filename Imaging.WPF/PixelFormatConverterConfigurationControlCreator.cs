using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Imaging;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class PixelFormatConverterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is PixelFormatConverterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PixelFormatConverterConfigurationControl() { DataContext = configuration };
    }
}
