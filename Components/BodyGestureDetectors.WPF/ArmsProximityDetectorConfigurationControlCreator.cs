using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.BodyGestureDetectors;

namespace OpenSense.WPF.Components.BodyGestureDetectors {
    [Export(typeof(IConfigurationControlCreator))]
    public class ArmsProximityDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ArmsProximityDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ArmsProximityDetectorConfigurationControl() { DataContext = configuration };
    }
}
