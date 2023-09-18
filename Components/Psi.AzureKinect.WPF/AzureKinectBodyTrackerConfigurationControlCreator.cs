using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.Psi.AzureKinect;

namespace OpenSense.WPF.Components.Psi.AzureKinect {
    [Export(typeof(IConfigurationControlCreator))]
    public class AzureKinectBodyTrackerConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is AzureKinectBodyTrackerConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new AzureKinectBodyTrackerConfigurationControl() { DataContext = configuration };
    }
}
