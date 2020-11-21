using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Imaging;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Imaging.Visualizer {
    [Export(typeof(IConfigurationControlCreator))]
    public class FlipColorVideoConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FlipColorVideoConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FlipColorVideoConfigurationControl() { DataContext = configuration };
    }
}
