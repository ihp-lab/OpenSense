using System.Windows.Controls;
using Microsoft.Psi;

namespace OpenSense.WPF.Components.Psi {
    public class DeliveryPolicyComboBox : ComboBox {

        public DeliveryPolicyComboBox() {
            Items.Add(new ComboBoxItem() { 
                Content = "Pipeline Default",
                Tag = null,
                ToolTip = "If set for Pipeline, this is equivalent to Unlimited; if set for connections, this is equivalent to the delivery policy of Pipeline.",
            });
            Items.Add(new ComboBoxItem() {
                Content = "Unlimited",
                Tag = DeliveryPolicy.Unlimited,
                ToolTip = "A lossless, unlimited delivery policy which lets the receiver queue grow as much as needed, with no latency constraints.",
            });
            Items.Add(new ComboBoxItem() {
                Content = "Latest Message",
                Tag = DeliveryPolicy.LatestMessage,
                ToolTip = "A lossy delivery policy which limits the receiver queue to one message, with no latency constraints.",
            });
            Items.Add(new ComboBoxItem() {
                Content = "Throttle",
                Tag = DeliveryPolicy.Throttle,
                ToolTip = "A throttling delivery policy, which attempts to throttle its source as long as there is a message in the queue waiting to be processed.",
            });
            Items.Add(new ComboBoxItem() {
                Content = "Synchronous or Throttle",
                Tag = DeliveryPolicy.SynchronousOrThrottle,
                ToolTip = "A delivery policy which attempts synchronous message delivery; if synchronous delivery fails, the source is throttled.",
            });
        }
    }
}
