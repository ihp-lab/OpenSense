using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PsiPipeline = Microsoft.Psi.Pipeline;
namespace OpenSense.Component.Contract {
    /// <summary>
    /// Base class of component configurations.
    /// </summary>
    [Serializable]
    public abstract class ComponentConfiguration : INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Guid id = Guid.NewGuid();

        public Guid Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string name = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string description = string.Empty;

        /// <summary>
        /// Display description. Can be used as a memo field.
        /// </summary>
        public string Description {
            get => description;
            set => SetProperty(ref description, value);
        }

        private ObservableCollection<InputConfiguration> inputs = new ObservableCollection<InputConfiguration>();

        public ObservableCollection<InputConfiguration> Inputs {
            get => inputs;
            set => SetProperty(ref inputs, value);
        }

        public abstract IComponentMetadata GetMetadata();

        /// <summary>
        /// Instantiate a component. Inputs should be connected.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="instantiatedComponents"></param>
        /// <returns>Instantiated component object. Can be null.</returns>
        public abstract object Instantiate(PsiPipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents);

    }
}
