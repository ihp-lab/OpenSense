﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components;

namespace OpenSense.WPF.Pipeline {
    public partial class OutputSelectionControl : UserControl {

        private InputConfiguration InputConfiguration;
        private IReadOnlyList<ComponentConfiguration> Configurations;

        private class Selection {
            public ComponentConfiguration Configuration { get; set; }
            public IPortMetadata PortMetadata { get; set; }
            public object Index { get; set; }
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
                                   Index = inputConfiguration.RemotePort?.Index ?? d.Aggregation.DefaultPortIndex(),
                               }
                            )
                        );
            var localOutputs = configuration.FindOutputPortDataTypes(configurations);
            var localInputs = configuration.FindInputPortDataTypes(configurations, inputMetadata);
            var localDataType = new RuntimePortDataType(inputMetadata, configuration.FindInputPortDataType(inputMetadata, configurations));
            var result = selections.Where(sel => {
                var remoteInputs = sel.Configuration.FindInputPortDataTypes(configurations);
                var remoteOutputs = sel.Configuration.FindOutputPortDataTypes(configurations, sel.PortMetadata);
                var remoteDataType = new RuntimePortDataType(sel.PortMetadata, sel.PortMetadata.GetTransmissionDataType(localDataType, remoteInputs, remoteOutputs));//try connect
                return inputMetadata.CanConnectDataType(remoteDataType, localOutputs, localInputs);
            }).ToArray();
            return result;
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

            var inputMetadata = configuration.FindPortMetadata(inputConfiguration.LocalPort);
            var inputDataTypeName = TypeDisplayHelpers.FindInputDataTypeName(configuration, inputMetadata, configurations);
            TextBlockPortDataType.Text = inputDataTypeName;

            var selections = LegalOutputSelections(configuration, inputConfiguration, configurations);
            ListBoxOutputs.ItemsSource = selections;
            var previousSelection = selections.SingleOrDefault(sel => inputConfiguration.RemoteId == sel.Configuration.Id && Equals(inputConfiguration.RemotePort.Identifier, sel.PortMetadata.Identifier));
            ListBoxOutputs.SelectedItem = previousSelection;
        }

        private void ListBoxOutputs_SelectionChanged(object sender, RoutedEventArgs e) {
            var selection = (Selection)ListBoxOutputs.SelectedItem;
            if (selection is null) {
                return;
            }
            InputConfiguration.RemoteId = selection.Configuration.Id;
            InputConfiguration.RemotePort = new PortConfiguration() {
                Identifier = selection.PortMetadata.Identifier,
                Index = selection.Index,
            };
            RemoveIllegalInputs(Configurations);
        }
    }
}
