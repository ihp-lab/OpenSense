using System.Composition;
using System.Windows;
using OpenSense.Component.GoogleCloud.Speech.V1;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.GoogleCloud.Speech.V1 {
    [Export(typeof(IInstanceControlCreator))]
    public class GoogleCloudSpeechInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) => instance is GoogleCloudSpeech;

        public UIElement Create(object instance) => new GoogleCloudSpeechInstanceControl() { DataContext = instance };
    }
}
