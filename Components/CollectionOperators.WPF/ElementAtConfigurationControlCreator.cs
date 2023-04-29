using System.Composition;
using System.Windows;
using OpenSense.Components.CollectionOperators;
using OpenSense.Components.Contract;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.CollectionOperators {
    [Export(typeof(IConfigurationControlCreator))]
    public class ElementAtConfigurationControlCreator : IConfigurationControlCreator {

        public bool CanCreate(ComponentConfiguration configuration) => configuration is ElementAtConfiguration;

        public UIElement Create(ComponentConfiguration configuration) => new ElementAtConfigurationControl() { DataContext = configuration };
    }
}
