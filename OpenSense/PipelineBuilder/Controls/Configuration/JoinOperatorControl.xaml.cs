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
    public partial class JoinOperatorControl : UserControl {

        private JoinOperatorConfiguration Config => DataContext as JoinOperatorConfiguration;

        public JoinOperatorControl() {
            InitializeComponent();
        }

        private void ComboBoxPrimaryDeliveryPolicy_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var index = ComboBoxPrimaryDeliveryPolicy.Items.Cast<ComboBoxItem>().ToList().FindIndex(i => i.Tag as DeliveryPolicy == Config?.PrimaryDeliveryPolicy);
            ComboBoxPrimaryDeliveryPolicy.SelectedIndex = index >= 0 ? index : 0;
        }

        private void ComboBoxPrimaryDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Config != null) {
                Config.PrimaryDeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxPrimaryDeliveryPolicy.SelectedItem).Tag);
            }
        }

        private void ComboBoxSecondaryDeliveryPolicy_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var index = ComboBoxSecondaryDeliveryPolicy.Items.Cast<ComboBoxItem>().ToList().FindIndex(i => i.Tag as DeliveryPolicy == Config?.SecondaryDeliveryPolicy);
            ComboBoxSecondaryDeliveryPolicy.SelectedIndex = index >= 0 ? index : 0;
        }

        private void ComboBoxSecondaryDeliveryPolicy_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Config != null) {
                Config.SecondaryDeliveryPolicy = ((DeliveryPolicy)((ComboBoxItem)ComboBoxSecondaryDeliveryPolicy.SelectedItem).Tag);
            }
        }
    }
}
