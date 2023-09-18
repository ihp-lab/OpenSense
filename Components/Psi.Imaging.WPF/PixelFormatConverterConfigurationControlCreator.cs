using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Imaging;

namespace OpenSense.WPF.Components.Psi.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class PixelFormatConverterConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is PixelFormatConverterConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PixelFormatConverterConfigurationControl() { DataContext = configuration };
    }
}
