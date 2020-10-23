using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Data;

namespace OpenSense.PipelineBuilder {
    [Serializable]
    public abstract class InstanceConfiguration : INotifyPropertyChanged {

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Type Type => GetType();

        private Guid guid = Guid.NewGuid();

        public Guid Guid {
            get => guid;
            set => SetProperty(ref guid, value);
        }

        private string name;

        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        private ObservableCollection<InputConfiguration> inputs = new ObservableCollection<InputConfiguration>();

        public ObservableCollection<InputConfiguration> Inputs {
            get => inputs;
            set => SetProperty(ref inputs, value);
        }

    }

    public abstract class ComponentConfiguration : InstanceConfiguration {

        public abstract object Instantiate(Pipeline pipeline);

    }

    public abstract class OperatorConfiguration : InstanceConfiguration {

        public object Instantiate(Pipeline pipeline, IList<InstanceEnvironment> instances) {
            var inputs = new List<dynamic>();
            foreach (var inConfig in Inputs) {
                var remote = instances.Single(inst => inst.Configuration.Guid == inConfig.Remote);
                var remoteDesc = ConfigurationManager.Description(remote.Configuration);
                var output = remoteDesc.Outputs.Single(o => o.Name == inConfig.Output.PropertyName);
                switch (output) {
                    case VirtualPortDescription vOut:
                        inputs.Add(remote.Instance);
                        break;
                    case StaticPortDescription sOut:
                        dynamic instProp = sOut.Property.GetValue(remote.Instance);
                        if (sOut.IsList) {
                            inputs.Add(instProp[int.Parse(inConfig.Output.Indexer)]);
                        } else if (sOut.IsDictionary) {
                            inputs.Add(instProp[inConfig.Output.Indexer]);
                        } else {
                            inputs.Add(instProp);
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            return Instantiate(inputs);
        }

        public abstract object Instantiate(IList<dynamic> inputs);

        public virtual Type OutputDataType(params Type[] inputTypes) {
            switch (inputTypes.Length) {
                case 2:
                    return typeof(ValueTuple<,>).MakeGenericType(inputTypes);
                default:
                    return null;
            }
        }

    }

    public abstract class ExporterConfiguration : InstanceConfiguration {

        public abstract object Instantiate(Pipeline pipeline);
    }

    public abstract class StreamWriterConfiguration : InstanceConfiguration {

        public abstract void Instantiate(Pipeline pipeline, IList<InstanceEnvironment> instances);
    }

    public abstract class ImporterConfiguration : InstanceConfiguration {

        public abstract object Instantiate(Pipeline pipeline);
    }

    public abstract class StreamReaderConfiguration : InstanceConfiguration {

        public abstract object Instantiate(Pipeline pipeline, IList<InstanceEnvironment> importers, Type dataType);
    }
}
