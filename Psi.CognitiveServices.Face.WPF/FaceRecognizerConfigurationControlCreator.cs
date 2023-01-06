using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.Psi.CognitiveServices.Face;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.Psi.CognitiveServices.Face {
    [Export(typeof(IConfigurationControlCreator))]
    public class FaceRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is FaceRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new FaceRecognizerConfigurationControl() { DataContext = configuration };
    }
}
