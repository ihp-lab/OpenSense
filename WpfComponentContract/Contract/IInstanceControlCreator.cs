using System.Windows;

namespace OpenSense.WPF.Component.Contract {
    public interface IInstanceControlCreator {

        bool CanCreate(object instance);

        UIElement Create(object instance);
    }
}
