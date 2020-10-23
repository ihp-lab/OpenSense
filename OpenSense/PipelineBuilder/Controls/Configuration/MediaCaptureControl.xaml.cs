using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenSense.PipelineBuilder.Configurations;
using OpenSense.Utilities;

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class MediaCaptureControl : UserControl {

        private MediaCaptureConfiguration Config => DataContext as MediaCaptureConfiguration;

        public MediaCaptureControl() {
            InitializeComponent();
        }

        private void ComboBoxCamera_Loaded(object sender, RoutedEventArgs e) {
            var devs = VideoDevice.Devices().ToList();
            ComboBoxCamera.ItemsSource = devs;
            ComboBoxCamera.DisplayMemberPath = nameof(VideoDevice.Name);
            var devIndex = devs.FindIndex(dev => dev.SymbolicLink == Config.Raw.DeviceId);
            if (devIndex >= 0) {
                var res = devs[devIndex].Resolutions.ToList();
                ComboBoxResolution.ItemsSource = res;
                var resIndex = res.FindIndex(r => r.Width == Config.Raw.Width && r.Height == Config.Raw.Height);
                ComboBoxResolution.SelectedIndex = resIndex >= 0 ? resIndex : 0;
            }
            ComboBoxCamera.SelectedIndex = devIndex >= 0 ? devIndex : 0;
        }

        private void ComboBoxCamera_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxCamera.SelectedItem is VideoDevice dev) {
                Config.Raw.DeviceId = dev.SymbolicLink;
                ComboBoxResolution.ItemsSource = dev.Resolutions;
                if (ComboBoxResolution.SelectedItem is Resolution res && dev.Resolutions.Contains(res)) {
                    return;
                }
            }
            ComboBoxResolution.SelectedIndex = 0;
        }

        private void ComboBoxResolution_Loaded(object sender, RoutedEventArgs e) {

        }

        private void ComboBoxResolution_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxResolution.SelectedItem is Resolution res) {
                Config.Raw.Width = res.Width;
                Config.Raw.Height = res.Height;
            }
        }

        private void ComboBoxFrameRate_Loaded(object sender, RoutedEventArgs e) {
            var item = ComboBoxFrameRate.Items.Cast<ComboBoxItem>().SingleOrDefault(i => double.Parse((string)i.Tag) == Config.Raw.Framerate);
            if (item is null) {
                ComboBoxFrameRate.SelectedIndex = 0;
            } else {
                ComboBoxFrameRate.SelectedItem = item;   
            }
        }

        private void ComboBoxFrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxFrameRate.SelectedItem is ComboBoxItem item && item.Tag is string s && double.TryParse(s, out var rate)) {
                Config.Raw.Framerate = rate;
            }
        }

        
    }
}
