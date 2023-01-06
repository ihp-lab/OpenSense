using System.Windows;
using OpenSense.Components.Contract;

namespace OpenSense.WPF.Components.Contract {
    public interface IConfigurationControlCreator {

        bool CanCreate(ComponentConfiguration configuration);

        UIElement Create(ComponentConfiguration configuration);
    }
}
