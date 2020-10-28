using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Component.Contract;
using OpenSense.Component.Contract.SpecialPortDataTypeIdentifiers;
using static OpenSense.Wpf.Pipeline.PortDataTypeFinder;

namespace OpenSense.Wpf.Pipeline {
    public partial class OutputSelectionControl : UserControl {

        private InputConfiguration InputConfiguration;
        private IReadOnlyList<ComponentConfiguration> Configurations;

        private class Selection {
            public ComponentConfiguration Configuration { get; set; }
            public IPortMetadata PortMetadata { get; set; }
            public object Index { get; set; }
        }

        private static object AssignIndexer(IPortMetadata metadata, PortConfiguration configuration) {
            switch (metadata.Aggregation) {
                case PortAggregation.List:
                case PortAggregation.Dictionary:
                    return configuration?.Index ?? null;
                case PortAggregation.Object:
                    return null;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static IList<Selection> LegalOutputSelections(ComponentConfiguration configuration, InputConfiguration inputConfiguration, IReadOnlyList<ComponentConfiguration> configurations) {
            var inputMetadata = configuration.GetMetadata().FindPortMetadata(inputConfiguration.LocalPort);
            var selections = configurations
                        .Where(inst => inst != configuration)
                        .SelectMany(i =>
                            i.GetMetadata().OutputPorts().Select(d =>
                               new Selection {
                                   Configuration = i,
                                   PortMetadata = d,
                                   Index = AssignIndexer(d, inputConfiguration.RemotePort),
                               }
                            )
                        );
            var localDataType = FindInputPortDataType(configuration, inputMetadata, configurations);
            if (typeof(Any).IsAssignableFrom(localDataType)) {
                return selections.ToArray();
            }
            return selections.Where(sel => {
                var remoteInputs = FindInputPortDataTypes(sel.Configuration, configurations);
                var remoteOutputs = FindOutputPortDataTypes(sel.Configuration, configurations, sel.PortMetadata);
                var remoteDataType = sel.PortMetadata.DataType(localDataType, remoteInputs, remoteOutputs);
                Type tempLocalDataType;
                if (localDataType != null) {
                    tempLocalDataType = localDataType;
                } else if (remoteDataType != null){
                    var localOutputs = FindOutputPortDataTypes(configuration, configurations);
                    var localInputs = FindInputPortDataTypes(configuration, configurations, inputMetadata);
                    tempLocalDataType = inputMetadata.DataType(remoteDataType, localOutputs, localInputs);
                } else {
                    tempLocalDataType = null;
                }
                return remoteDataType != null && tempLocalDataType != null && tempLocalDataType.IsAssignableFrom(remoteDataType);
            }).ToArray();
        }

        private static void RemoveIllegalInputs(IReadOnlyList<ComponentConfiguration> configurations) {
            var flag = true;//remove connections that become illegal
            while (flag) {
                flag = false;
                foreach (var instance in configurations) {
                    foreach (var inputConfiguration in instance.Inputs) {
                        var selections = LegalOutputSelections(instance, inputConfiguration, configurations);
                        if (selections.Count == 0 || //no selection
                            selections.All(s => s.Configuration.Id != inputConfiguration.RemoteId || !Equals(s.PortMetadata.Identifier, inputConfiguration.RemotePort.Identifier)) // no connection
                            ) {
                            instance.Inputs.Remove(inputConfiguration);
                            flag = true;
                            goto jump;
                        }
                    }
                }
jump:
                ;
            }
        }

        public OutputSelectionControl(ComponentConfiguration configuration, InputConfiguration inputConfiguration, IReadOnlyList<ComponentConfiguration> configurations) {
            InitializeComponent();

            InputConfiguration = inputConfiguration;
            Configurations = configurations;

            var inputMetadata = configuration.GetMetadata().FindPortMetadata(inputConfiguration.LocalPort);
            var localOutputs = FindOutputPortDataTypes(configuration, configurations);
            var localInputs = FindInputPortDataTypes(configuration, configurations, inputMetadata);
            var inputPortDataType = inputMetadata.DataType(null, localOutputs, localInputs);
            string inputDataType;
            if (inputPortDataType is null) {
                inputDataType = "Unknown";
            } else if (typeof(Any).IsAssignableFrom(inputPortDataType)) {
                inputDataType = "Any";
            } else {
                inputDataType = inputPortDataType.FullName;
            }
            TextBlockPortDataType.Text = inputDataType;

            var selections = LegalOutputSelections(configuration, inputConfiguration, configurations);
            ListBoxOutputs.ItemsSource = selections;
            ListBoxOutputs.SelectedItem = selections.SingleOrDefault(sel => inputConfiguration.RemoteId == sel.Configuration.Id && Equals(inputConfiguration.RemotePort.Identifier, sel.PortMetadata.Identifier));
        }

        private void ListBoxOutputs_SelectionChanged(object sender, RoutedEventArgs e) {
            var selection = (Selection)ListBoxOutputs.SelectedItem;
            if (selection != null) {
                InputConfiguration.RemoteId = selection.Configuration.Id;
                InputConfiguration.RemotePort = new PortConfiguration() {
                    Identifier = selection.PortMetadata.Identifier,
                    Index = selection.Index,
                };
                RemoveIllegalInputs(Configurations);
            }
        }
    }
}
