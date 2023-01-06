using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Imaging;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class FlipColorVideoConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FlipColorVideoConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FlipColorVideoConfigurationControl() { DataContext = configuration };
    }
}
