using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Builtin;

namespace OpenSense.WPF.Components.Builtin {
    [Export(typeof(IConfigurationControlCreator))]
    public class DeduplicatorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is DeduplicatorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new DeduplicatorConfigurationControl() {
            DataContext = configuration
        };
    }
}
