using System.Windows.Controls;
using Microsoft.Psi;

namespace OpenSense.WPF.Components.Psi.Common {
    public class DeliveryPolicyComboBox : ComboBox {

        public DeliveryPolicyComboBox() {
            Items.Add(new ComboBoxItem() { 
                Content = "Pipeline default",
                Tag = null,
            });
            Items.Add(new ComboBoxItem() {
                Content = "Unlimited",
                Tag = DeliveryPolicy.Unlimited,
            });
            Items.Add(new ComboBoxItem() {
                Content = "Latest message",
                Tag = DeliveryPolicy.LatestMessage,
            });
            Items.Add(new ComboBoxItem() {
                Content = "Throttle",
                Tag = DeliveryPolicy.Throttle,
            });
            Items.Add(new ComboBoxItem() {
                Content = "Synchronous or throttle",
                Tag = DeliveryPolicy.SynchronousOrThrottle,
            });
        }
    }
}
