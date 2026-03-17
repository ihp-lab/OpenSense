#nullable enable

using System;
using System.Windows;
using System.Windows.Controls;
using HMInterop;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class HevcEncoderConfigurationControl : UserControl {

        private HevcEncoderConfiguration? Configuration => DataContext as HevcEncoderConfiguration;

        public HevcEncoderConfigurationControl() {
            InitializeComponent();
        }

        #region Control Event Handlers
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

            var copyCount = Math.Min(oldSize, newSize);
            for (var i = 0; i < copyCount; i++) {
                newEntries[i] = oldEntries![i];
            }

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
                entry.ReferencePics[0] = -1;
                entry.UsedByCurrPic[0] = 1;
                entry.NumRefPics = 1;
                newEntries[i] = entry;
            }

            raw.GOPEntries = newEntries;

            TextBoxGOPSize.Text = newSize.ToString();
            DataGridGOP.ItemsSource = null;
            DataGridGOP.ItemsSource = raw.GOPEntries;
        }
        #endregion
    }
}
