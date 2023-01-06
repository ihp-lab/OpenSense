#nullable enable

using System.Globalization;
using System.Windows.Controls;
using OpenSense.Components.MediaPipe.NET;

namespace OpenSense.WPF.Components.MediaPipe.NET {
    public partial class SidePacketConfigurationControl : UserControl {

        private SidePacketConfiguration Configuration => (SidePacketConfiguration)DataContext;

        public SidePacketConfigurationControl() {
            InitializeComponent();
        }

        private void comboBoxDataType_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ruleJToken.PacketType = Configuration.PacketType;

            var json = convJToken.Convert(Configuration.Value, typeof(string), null, CultureInfo.InvariantCulture);
            var valid = ruleJToken.Validate(json, CultureInfo.InvariantCulture);
            if (!valid.IsValid) {
                Configuration.Value = MediaPipeInteropHelpers.CreateDefaultValue(Configuration.PacketType);
            }
        }
    }
}
