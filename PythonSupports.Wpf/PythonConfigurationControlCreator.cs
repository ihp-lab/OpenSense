using System.Composition;
using System.Windows;
using OpenSense.Component.Contract;
using OpenSense.Component.PythonSupports;
using OpenSense.Wpf.Component.Contract;

namespace OpenSense.Wpf.Component.PythonSupports {
    [Export(typeof(IConfigurationControlCreator))]
    public class PythonConfigurationControlCreator : IConfigurationControlCreator {
        public bool CanCreate(ComponentConfiguration configuration) => configuration is PythonConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new PythonConfigurationControl() { 
            DataContext = configuration,
        };
    }
}
