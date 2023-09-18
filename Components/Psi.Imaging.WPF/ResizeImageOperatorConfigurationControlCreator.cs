using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.Imaging;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.Psi.Imaging {
    [Export(typeof(IConfigurationControlCreator))]
    public class ResizeImageOperatorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ResizeImageOperatorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ResizeImageOperatorConfigurationControl() { DataContext = configuration };
    }
}
