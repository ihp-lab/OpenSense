using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.OpenPose;


namespace OpenSense.WPF.Components.OpenPose {
    [Export(typeof(IConfigurationControlCreator))]
    public class OpenPoseConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is OpenPoseConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new OpenPoseConfigurationControl() { DataContext = configuration };
    }
}
