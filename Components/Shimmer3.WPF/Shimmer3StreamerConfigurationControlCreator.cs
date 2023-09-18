using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Shimmer3;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Shimmer3 {
    [Export(typeof(IConfigurationControlCreator))]
    public class Shimmer3StreamerConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is Shimmer3StreamerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new Shimmer3StreamerConfigurationControl { 
            DataContext = configuration,
        };
    }
}
