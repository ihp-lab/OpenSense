using System.Composition;
using System.Windows;
using OpenSense.Components.Contract;
using OpenSense.Components.OpenPose;
using OpenSense.WPF.Components.Contract;


namespace OpenSense.WPF.Components.OpenPose {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenPoseConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenPoseConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenPoseConfigurationControl() { DataContext = configuration };
    }
}
