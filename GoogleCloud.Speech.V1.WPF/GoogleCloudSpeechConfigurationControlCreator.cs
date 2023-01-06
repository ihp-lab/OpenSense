using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.GoogleCloud.Speech.V1;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.GoogleCloud.Speech.V1 {
    [Export(typeof(IConfigurationControlCreator))]
    public class GoogleCloudSpeechConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is GoogleCloudSpeechConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new GoogleCloudSpeechConfigurationControl() { DataContext = configuration };
    }
}
