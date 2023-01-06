using System.Windows;

namespace OpenSense.WPF.Components.Contract {
    public interface IInstanceControlCreator {

        bool CanCreate(object instance);

        UIElement Create(object instance);
    }
}
