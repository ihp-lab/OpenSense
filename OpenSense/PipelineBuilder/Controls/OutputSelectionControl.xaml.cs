using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;

namespace OpenSense.PipelineBuilder.Controls {
    public partial class OutputSelectionControl : UserControl {

        private InputConfiguration Config;
        private IList<InstanceConfiguration> Instances;

        private class Selection {
            public InstanceConfiguration Instance { get; set; }
            public PortDescription Port { get; set; }
            public string Indexer { get; set; }
        }

        private static string AssignIndexer(PortDescription desc, PortConfiguration config) {
            if (desc.IsList || desc.IsDictionary) {
                return config is null || config.Indexer is null ? string.Empty : config.Indexer;
            }
            return null;
        }

        private static IList<Selection> LegalSelections(InstanceConfiguration instConfig, InputConfiguration inConfig, IList<InstanceConfiguration> instances) {
            var instDesc = ConfigurationManager.Description(instConfig);
            var inputDesc = instDesc.Inputs.Single(i => i.Name == inConfig.Input.PropertyName);
            var selections = instances
                        .Where(inst => inst != instConfig)
                        .Select(inst => (inst, ConfigurationManager.Description(inst)))
                        .SelectMany(t => 
                            t.Item2.Outputs.Select(
                                d => new Selection { 
                                    Instance = t.inst, Port = d, 
                                    Indexer = AssignIndexer(d, inConfig.Output) 
                                }
                            )
                        )
                        .ToList();
            switch (inputDesc) {
                case VirtualPortDescription vIn: // show all
                    break;
                case StaticPortDescription sIn: //show only type matched
                    selections = selections
                        .Where(sel => {
                            switch (sel.Port) {
                                case VirtualPortDescription vOut:
                                    var vOutType = vOut.DataType(sel.Instance, instances);
                                    return vOutType == typeof(Any) || sIn.DataType.IsAssignableFrom(vOutType);//if vOutType is null, false is returned
                                case StaticPortDescription sOut:
                                    return sIn.DataType.IsAssignableFrom(sOut.DataType);
                                default:
                                    throw new InvalidOperationException();
                            }
                        }).ToList();
                    break;
            }
            return selections;
        }

        private static string InputPortDataType(InstanceConfiguration instConfig, InputConfiguration inConfig) {
            var instDesc = ConfigurationManager.Description(instConfig);
            var inputDesc = instDesc.Inputs.Single(i => i.Name == inConfig.Input.PropertyName);
            switch (inputDesc) {
                case VirtualPortDescription vIn: // show all
                    return typeof(Any).Name;
                case StaticPortDescription sIn: //show only type matched
                    return sIn.DataType.ToString();
                default:
                    throw new InvalidOperationException();
            }
        }

        private static void CleanUpConnections(IList<InstanceConfiguration> instances) {
            var flag = true;//remove connections that become illegal
            while (flag) {
                flag = false;
                foreach (var instConfig in instances) {
                    foreach (var inConfig in instConfig.Inputs) {
                        var selections = LegalSelections(instConfig, inConfig, instances);
                        if (selections.Count == 0 || selections.All(s => s.Instance.Guid != inConfig.Remote || s.Port.Name != inConfig.Output.PropertyName)) {
                            instConfig.Inputs.Remove(inConfig);
                            flag = true;
                            goto jump;
                        }
                    }
                }
jump:
                ;
            }
        }

        public OutputSelectionControl(InstanceConfiguration instConfig, InputConfiguration inConfig, IList<InstanceConfiguration> instances) {
            InitializeComponent();

            Config = inConfig;
            Instances = instances;

            TextBlockPortDataType.Text = InputPortDataType(instConfig, inConfig);

            var selections = LegalSelections(instConfig, inConfig, instances);

            ListBoxOutputs.ItemsSource = selections;
            ListBoxOutputs.SelectedItem = selections.SingleOrDefault(sel => inConfig.Remote == sel.Instance.Guid && inConfig.Output.PropertyName == sel.Port.Name);
        }

        private void ListBoxOutputs_SelectionChanged(object sender, RoutedEventArgs e) {
            var selection = (Selection)ListBoxOutputs.SelectedItem;
            if (selection != null) {
                Config.Remote = selection.Instance.Guid;
                Config.Output = new PortConfiguration() {
                    PropertyName = selection.Port.Name,
                    Indexer = selection.Port.IsList || selection.Port.IsDictionary ? selection.Indexer : null,
                };
                CleanUpConnections(Instances);
            }
        }
    }
}
