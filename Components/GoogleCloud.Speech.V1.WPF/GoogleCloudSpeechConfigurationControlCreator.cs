using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.GoogleCloud.Speech.V1;

namespace OpenSense.WPF.Components.GoogleCloud.Speech.V1 {
    [Export(typeof(IConfigurationControlCreator))]
    public class GoogleCloudSpeechConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is GoogleCloudSpeechConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new GoogleCloudSpeechConfigurationControl() { DataContext = configuration };
    }
}
