using System.Composition;
using System.Windows;
using OpenSense.Component.BodyGestureDetectors;
using OpenSense.Component.Contract;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.BodyGestureDetectors {
    [Export(typeof(IConfigurationControlCreator))]
    public class BodyLeaningDetectorConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is BodyLeaningDetectorConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new BodyLeaningDetectorConfigurationControl() { DataContext = configuration };
    }
}
