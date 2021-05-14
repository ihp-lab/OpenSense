using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.OpenFace;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.OpenFace {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenFaceConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenFaceConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenFaceConfigurationControl() { DataContext = configuration };
    }
}
