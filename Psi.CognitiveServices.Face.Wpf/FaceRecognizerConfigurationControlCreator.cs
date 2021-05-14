using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.CognitiveServices.Face;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.Psi.CognitiveServices.Face {
    [Export(typeof(IConfigurationControlCreator))]
    public class FaceRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FaceRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FaceRecognizerConfigurationControl() { DataContext = configuration };
    }
}
