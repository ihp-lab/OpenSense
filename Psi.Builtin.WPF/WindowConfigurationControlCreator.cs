using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi {
    [Export(typeof(IConfigurationControlCreator))]
    public class WindowConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is WindowConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new WindowConfigurationControl() { DataContext = configuration};
    }
}
