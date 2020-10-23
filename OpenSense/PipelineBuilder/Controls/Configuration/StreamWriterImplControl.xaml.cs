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
    public partial class StreamWriterImplControl : UserControl {
        public StreamWriterImplControl(IList<InstanceConfiguration> instances) {
            Exporters = instances.OfType<ExporterConfiguration>().ToList();

            InitializeComponent();
        }

        private List<ExporterConfiguration> Exporters;

        private StreamWriterImplConfiguration Config => DataContext as StreamWriterImplConfiguration;

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            ComboBoxDeliveryPolicy.SelectedItem = ComboBoxDeliveryPolicy.Items.Cast<ComboBoxItem>().Single(i => i.Tag as DeliveryPolicy == Config?.DeliveryPolicy);

            ListBoxExporters.DisplayMemberPath = nameof(ExporterConfiguration.Name);
            ListBoxExporters.ItemsSource = Exporters;
            ListBoxExporters.SelectedItem = Exporters.SingleOrDefault(i => i.Guid == Config?.Exporter);
            if (ListBoxExporters.SelectedItem is null && Config != null) {
                Config.Exporter = Guid.Empty;
            }

            ListBoxExporters.Visibility = Exporters.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TextBlockExporters.Visibility = Exporters.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ComboBoxDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Config != null) {
                Config.DeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxDeliveryPolicy.SelectedItem).Tag);
            }
        }

        private void ListBoxExporters_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Config != null) {
                Config.Exporter = ((ExporterConfiguration)ListBoxExporters.SelectedItem).Guid;
            }
        }
    }
}
