#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components;
using OpenSense.WPF.Pipeline;

namespace OpenSense.WPF.Views.Editors {

    public sealed partial class OutputPortsInspector : UserControl {

        private readonly ComponentConfiguration _configuration;

        private readonly IReadOnlyList<ComponentConfiguration> _configurations;

        public OutputPortsInspector(ComponentConfiguration configuration, IReadOnlyList<ComponentConfiguration> configurations) {

            _configuration = configuration;
            _configurations = configurations;

            InitializeComponent();

            /* Create view models */
            UpdateViewModels();

            /* Handle connection changes */
            _configuration.Inputs.CollectionChanged += OnCollectionChanged;
            foreach (var input in _configuration.Inputs) {
                input.PropertyChanged += OnPropertyChanged;
            }
        }

        #region View Model Event Handlers
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) {
            if (args.OldItems is not null) {
                foreach (var removed in args.OldItems.Cast<InputConfiguration>()) {
                    removed.PropertyChanged -= OnPropertyChanged;
                }
            }

            if (args.NewItems is not null) {
                foreach (var @new in args.NewItems.Cast<InputConfiguration>()) {
                    @new.PropertyChanged += OnPropertyChanged;
                } 
            }

            UpdateViewModels();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs args) {
            UpdateViewModels();
        }
        #endregion

        #region Control Event Handlers
        private void UserControl_Unloaded(object sender, System.Windows.RoutedEventArgs e) {
            _configuration.Inputs.CollectionChanged -= OnCollectionChanged;
            foreach (var input in _configuration.Inputs) {
                input.PropertyChanged -= OnPropertyChanged;
            }
        }
        #endregion

        #region Helpers
        private void UpdateViewModels() {
            var metadata = _configuration.GetMetadata();
            var outputs = metadata.OutputPorts().ToArray();
            if (outputs.Length == 0) {
                TextBlockNoOutputs.Visibility = Visibility.Visible;
                ListBoxPorts.Visibility = Visibility.Collapsed;
                ListBoxPorts.ItemsSource = null;
                return;
            }
            var models = outputs
                .Select(p => {
                    var outputDataTypeName = TypeDisplayHelpers.FindOutputDataTypeName(_configuration, p, _configurations);
                    var result = new OutputPortViewModel(p.Name, p.Description, outputDataTypeName);
                    return result;
                })
                .ToArray();
            TextBlockNoOutputs.Visibility = Visibility.Collapsed;
            ListBoxPorts.Visibility = Visibility.Visible;
            ListBoxPorts.ItemsSource = models;
        }
        #endregion
    }
}
