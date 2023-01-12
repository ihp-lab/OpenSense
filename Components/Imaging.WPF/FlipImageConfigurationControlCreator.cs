using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Imaging;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class FlipImageConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FlipImageConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FlipImageConfigurationControl() { DataContext = configuration };
    }
}
