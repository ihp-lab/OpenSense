using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.OpenFace;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.OpenFace {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenFaceConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenFaceConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenFaceConfigurationControl() { DataContext = configuration };
    }
}
