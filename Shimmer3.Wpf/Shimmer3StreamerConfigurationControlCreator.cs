using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Shimmer3;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Shimmer3 {
    [Export(typeof(IConfigurationControlCreator))]
    public class Shimmer3StreamerConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is Shimmer3StreamerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new Shimmer3StreamerConfigurationControl { 
            DataContext = configuration,
        };
    }
}
