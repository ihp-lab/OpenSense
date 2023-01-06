using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.Component.Contract;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.BodyGestureDetectors {
    [Export(typeof(IConfigurationControlCreator))]
    public class ArmsProximityDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ArmsProximityDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ArmsProximityDetectorConfigurationControl() { DataContext = configuration };
    }
}
