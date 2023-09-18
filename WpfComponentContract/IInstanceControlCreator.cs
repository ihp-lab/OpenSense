using System.Windows;

namespace OpenSense.WPF.Components {
    public interface IInstanceControlCreator {

        bool CanCreate(object instance);

        UIElement Create(object instance);
    }
}
