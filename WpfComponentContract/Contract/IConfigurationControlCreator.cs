using System.Windows;
using OpenSense.Component.Contract;

namespace OpenSense.Wpf.Component.Contract {
    public interface IConfigurationControlCreator {

        bool CanCreate(ComponentConfiguration configuration);

        UIElement Create(ComponentConfiguration configuration);
    }
}
