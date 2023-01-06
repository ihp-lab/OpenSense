using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.Psi.AzureKinect;
using OpenSense.WPF.Component.Contract;

namespace OpenSense.WPF.Component.Psi.AzureKinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerConfigurationControl() { DataContext = configuration };
    }
}
