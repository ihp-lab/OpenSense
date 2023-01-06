using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Imaging;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class FlipColorVideoConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FlipColorVideoConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FlipColorVideoConfigurationControl() { DataContext = configuration };
    }
}
