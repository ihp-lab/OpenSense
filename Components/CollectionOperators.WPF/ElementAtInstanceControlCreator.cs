using System.Composition;
using System.Windows;
using OpenSense.Components.CollectionOperators;
using OpenSense.WPF.Components.Contract;

namespace OpenSense.WPF.Components.CollectionOperators {
    [Export(typeof(IInstanceControlCreator))]
    public class ElementAtInstanceControlCreator : IInstanceControlCreator {

        public bool CanCreate(object instance) { 
            var type = instance.GetType();
            if (!type.IsGenericType) {
                return false;
            }
            var genericType = type.GetGenericTypeDefinition();
            var result = genericType == typeof(ElementAt<>);
            return result;
        }

        public UIElement Create(object instance) => new ElementAtInstanceControl() { DataContext = instance };
    }
}
