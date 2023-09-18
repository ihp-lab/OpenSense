using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class WindowConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is WindowConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WindowConfigurationControl() { DataContext = configuration};
    }
}
