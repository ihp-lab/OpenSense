using System.Composition;
using OpenSense.Components;
using System.Windows;
using OpenSense.Components.AzureSpeech;

namespace OpenSense.WPF.Components.AzureSpeech {
    [Export(typeof(IConfigurationControlCreator))]
    public sealed class AzureSpeechRecognizerConfigurationControlCreator : IConfigurationControlCreator {

        #region IConfigurationControlCreator
        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureSpeechRecognizerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureSpeechRecognizerConfigurationControl() {
            DataContext = configuration
        }; 
        #endregion
    }
}
