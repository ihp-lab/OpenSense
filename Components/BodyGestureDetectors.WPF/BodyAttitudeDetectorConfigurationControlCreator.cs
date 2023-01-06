using System.Composition;
using System.Windows;
using OpenSense.Components.BodyGestureDetectors;
using OpenSense.Components.Contract;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    [Export(typeof(IConfigurationControlCreator))]
    public class BodyAttitudeDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is BodyAttitudeDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new BodyAttitudeDetectorConfigurationControl() { DataContext = configuration };
    }
}
