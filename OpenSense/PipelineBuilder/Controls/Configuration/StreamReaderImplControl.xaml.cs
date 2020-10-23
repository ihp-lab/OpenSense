using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Psi;
using OpenSense.PipelineBuilder.Configurations;

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class StreamReaderImplControl : UserControl {
        public StreamReaderImplControl(IList<InstanceConfiguration> instances) {
            Importers = instances.OfType<ImporterConfiguration>().ToList();

            InitializeComponent();
        }

        private List<ImporterConfiguration> Importers;

        private StreamReaderImplConfiguration Config => DataContext as StreamReaderImplConfiguration;

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {

            ListBoxImporters.DisplayMemberPath = nameof(ImporterConfiguration.Name);
            ListBoxImporters.ItemsSource = Importers;
            ListBoxImporters.SelectedItem = Importers.SingleOrDefault(i => i.Guid == Config?.Importer);
            if (ListBoxImporters.SelectedItem is null && Config != null) {
                Config.Importer = Guid.Empty;
            }

            ListBoxImporters.Visibility = Importers.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TextBlockImporters.Visibility = Importers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ListBoxImporters_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Config != null) {
                Config.Importer = ((ImporterConfiguration)ListBoxImporters.SelectedItem).Guid;
            }
        }
    }
}
