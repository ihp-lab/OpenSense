using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.Component.Contract;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.BodyGestureDetectors {
    [Export(typeof(IConfigurationControlCreator))]
    public class BodyAttitudeDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is BodyAttitudeDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new BodyAttitudeDetectorConfigurationControl() { DataContext = configuration };
    }
}
