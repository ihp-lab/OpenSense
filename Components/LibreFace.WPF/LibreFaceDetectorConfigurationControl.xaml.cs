#nullable enable

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Psi;
using OpenSense.Components.LibreFace;

namespace OpenSense.WPF.Components.LibreFace {
    public sealed partial class LibreFaceDetectorConfigurationControl : UserControl {

        private LibreFaceDetectorConfiguration? Configuration => (LibreFaceDetectorConfiguration?)DataContext;

        public LibreFaceDetectorConfigurationControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e) {
            var index = ComboBoxDeliveryPolicy.Items.Cast<ComboBoxItem>().ToList().FindIndex(i => i.Tag as DeliveryPolicy == Configuration?.DeliveryPolicy);
            ComboBoxDeliveryPolicy.SelectedIndex = index >= 0 ? index : 0;
        }

        private void ComboBoxDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Configuration is null) {
                return;
            }
            Configuration.DeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxDeliveryPolicy.SelectedItem).Tag);
        }

        
    }
}
