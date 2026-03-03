#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using HMInterop;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class FileWriterConfigurationControl : UserControl {

        private FileWriterConfiguration? Configuration => DataContext as FileWriterConfiguration;

        public FileWriterConfigurationControl() {
            InitializeComponent();
        }

        #region Control Event Handlers
        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e) {
            if (Configuration is null) {
                return;
            }
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "mp4",
                Filter = "MP4 Video (*.mp4)|*.mp4|All Files (*.*)|*.*"
            };
            if (saveFileDialog.ShowDialog() == true) {
                Configuration.Filename = saveFileDialog.FileName;
            }
        }

        private void ButtonApplyGOPSize_Click(object sender, RoutedEventArgs e) {
            if (Configuration?.Raw is null) {
                return;
            }

            var raw = Configuration.Raw;
            if (!int.TryParse(TextBoxGOPSize.Text, out var newSize) || newSize < 1) {
                newSize = 1;
                TextBoxGOPSize.Text = "1";
            }

            var oldEntries = raw.GOPEntries;
            var oldSize = oldEntries?.Length ?? 0;

            if (newSize == oldSize) {
                return;
            }

            var newEntries = new GOPEntryConfig[newSize];

            // Copy existing entries
            var copyCount = Math.Min(oldSize, newSize);
            for (var i = 0; i < copyCount; i++) {
                newEntries[i] = oldEntries![i];
            }

            // Create new default entries for any added slots
            for (var i = copyCount; i < newSize; i++) {
                var entry = new GOPEntryConfig {
                    POC = i + 1,
                    SliceType = SliceType.B,
                    TemporalId = 0,
                    IsReferencePicture = true,
                    QPOffset = 1,
                    QPFactor = 0.4624,
                    NumRefPicsActive = 2,
                };
                // Default reference: previous frame
                entry.ReferencePics[0] = -1;
                entry.UsedByCurrPic[0] = 1;
                entry.NumRefPics = 1;
                newEntries[i] = entry;
            }

            raw.GOPEntries = newEntries;

            // Refresh DataGrid binding and GOP size display
            TextBoxGOPSize.Text = newSize.ToString();
            DataGridGOP.ItemsSource = null;
            DataGridGOP.ItemsSource = raw.GOPEntries;
        }
        #endregion
    }
}
