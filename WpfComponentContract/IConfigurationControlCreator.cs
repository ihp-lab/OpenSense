using System.Windows;
using OpenSense.Components;

namespace OpenSense.WPF.Components {
    public interface IConfigurationControlCreator {

        bool CanCreate(ComponentConfiguration configuration);

        UIElement Create(ComponentConfiguration configuration);
    }
}
