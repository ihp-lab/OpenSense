using System.Windows;

namespace OpenSense.Wpf.Component.Contract {
    public interface IInstanceControlCreator {

        bool CanCreate(object instance);

        UIElement Create(object instance);
    }
}
