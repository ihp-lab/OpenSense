using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.OpenPose;
using OpenSense.WPF.Component.Contract;


namespace OpenSense.WPF.Component.OpenPose {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenPoseConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenPoseConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenPoseConfigurationControl() { DataContext = configuration };
    }
}
