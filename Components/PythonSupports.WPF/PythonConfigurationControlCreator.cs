using System.Composition;
using System.Windows;
using OpenSense.Components;
using OpenSense.Components.PythonSupports;
using OpenSense.WPF.Components;

namespace OpenSense.WPF.Components.PythonSupports {
    [Export(typeof(IConfigurationControlCreator))]
    public class PythonConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is PythonConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PythonConfigurationControl() { 
            DataContext = configuration,
        };
    }
}
