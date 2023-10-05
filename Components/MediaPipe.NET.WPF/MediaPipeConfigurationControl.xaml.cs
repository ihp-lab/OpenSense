using System.IO;
using System.Windows.Controls;
using OpenSense.Components.MediaPipe.NET;

namespace OpenSense.WPF.Components.MediaPipe.NET {
    public sealed partial class MediaPipeConfigurationControl : UserControl {

        private MediaPipeConfiguration Configuration => (MediaPipeConfiguration)DataContext;

        public MediaPipeConfigurationControl() {
            InitializeComponent();
        }

        private void buttonAddSidePacket_Click(object sender, System.Windows.RoutedEventArgs e) {
            var packet = new SidePacketConfiguration();
            Configuration.InputSidePackets.Add(packet);
            listBoxSidePackets.SelectedItem = packet;
        }

        private void buttonRemoveSidePacket_Click(object sender, System.Windows.RoutedEventArgs e) {
            var packet = listBoxSidePackets.SelectedItem as SidePacketConfiguration;
            if (packet is null) {
                return;
            }
            Configuration.InputSidePackets.Remove(packet);
        }

        private void buttonAddInput_Click(object sender, System.Windows.RoutedEventArgs e) {
            var stream = new InputStreamConfiguration();
            Configuration.InputStreams.Add(stream);
            listBoxInputStreams.SelectedItem = stream;
        }

        private void buttonRemoveInput_Click(object sender, System.Windows.RoutedEventArgs e) {
            var stream = listBoxInputStreams.SelectedItem as InputStreamConfiguration;
            if (stream is null) {
                return;
            }
            Configuration.InputStreams.Remove(stream);
        }

        private void buttonAddOutput_Click(object sender, System.Windows.RoutedEventArgs e) {
            var stream = new OutputStreamConfiguration();
            Configuration.OutputStreams.Add(stream);
            listBoxOutputStreams.SelectedItem = stream;
        }

        private void buttonRemoveOutput_Click(object sender, System.Windows.RoutedEventArgs e) {
            var stream = listBoxOutputStreams.SelectedItem as OutputStreamConfiguration;
            if (stream is null) {
                return;
            }
            Configuration.OutputStreams.Remove(stream);
        }

        private void buttonOpenGraph_Click(object sender, System.Windows.RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.pbtxt",
                Filter = "MediaPipe Graph (*.pbtxt) | *.pbtxt",
                InitialDirectory = string.IsNullOrEmpty(Configuration.Graph) ? "" : Path.GetDirectoryName(Path.GetFullPath(Configuration.Graph)),
            };
            if (openFileDialog.ShowDialog() == true) {
                Configuration.Graph = openFileDialog.FileName;
            }
        }
    }
}
