using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Psi.Media_Interop;
using OpenSense.Component.Psi.Media;

namespace OpenSense.Wpf.Component.Psi.Media {
    public partial class MediaCaptureConfigurationControl : UserControl {

        private MediaCaptureConfiguration Config => DataContext as MediaCaptureConfiguration;

        public MediaCaptureConfigurationControl() {
            InitializeComponent();
        }

        private bool comboBoxCamera_Loaded = false;//Load() method will be called twice, we only want it to be called once. If called twice, it will have wrong behavior.

        private void ComboBoxCamera_Loaded(object sender, RoutedEventArgs e) {
            if (comboBoxCamera_Loaded) {
                return;
            }
            comboBoxCamera_Loaded = true;

            MediaCaptureDevice.Initialize();
            var newCameraComboBoxItems = new List<ComboBoxItem>();
            ComboBoxCamera.Items.Clear();
            foreach (var device in MediaCaptureDevice.AllDevices.OrderBy(d => d.FriendlyName)) {
                device.Attach(Config.Raw.UseInSharedMode);//required, otherwise no detail will show
                var item = new ComboBoxItem() { Content = device.FriendlyName, Tag = device };
                ComboBoxCamera.Items.Add(item);
            }
            foreach (var item in ComboBoxCamera.Items.Cast<ComboBoxItem>()) {
                var dev = (MediaCaptureDevice)item.Tag;
                if (Config.Raw.DeviceId != dev.SymbolicLink) {
                    continue;
                }
                ComboBoxCamera.SelectedItem = item;
                return;
            }
            Debug.Assert(ComboBoxCamera.SelectedItem is null);
            ComboBoxCamera.SelectedItem = ComboBoxCamera.Items.Cast<ComboBoxItem>().FirstOrDefault();
            ComboBoxCamera_SelectionChanged(sender, e);
        }

        private void ComboBoxCamera_SelectionChanged(object sender, RoutedEventArgs e) {
            if (ComboBoxCamera.SelectedItem is null) {
                Config.Raw.DeviceId = null;
                ComboBoxResolution.Items.Clear();
                if (ComboBoxResolution.SelectedItem is null) {
                    ComboBoxResolution_SelectionChanged(sender, e);
                } else {
                    ComboBoxResolution.SelectedItem = null;
                }
                ComboBoxResolution.IsEnabled = false;
                return;
            }
            var dev = (MediaCaptureDevice)((ComboBoxItem)ComboBoxCamera.SelectedItem).Tag;
            Config.Raw.DeviceId = dev.SymbolicLink;
            var resolutions = dev.Formats.Select(f => (f.nWidth, f.nHeight))
                .Distinct()
                .OrderByDescending(r => r, Comparer<(int width, int height)>.Create((a, b) => a.width != b.width ? a.width.CompareTo(b.width) : a.height.CompareTo(b.height)));
            ComboBoxResolution.Items.Clear();
            foreach (var resolution in resolutions) {
                var name = $"{resolution.nWidth} × {resolution.nHeight}";
                var item = new ComboBoxItem() { Content = name, Tag = resolution };
                ComboBoxResolution.Items.Add(item);
            }
            ComboBoxResolution.IsEnabled = ComboBoxResolution.Items.Count > 0;
            foreach (var item in ComboBoxResolution.Items.Cast<ComboBoxItem>()) {
                var (width, height) = (ValueTuple<int, int>)item.Tag;
                if (width != Config.Raw.Width || height != Config.Raw.Height) {
                    continue;
                }
                ComboBoxResolution.SelectedItem = item;
                return;
            }
            ComboBoxResolution.SelectedItem = ComboBoxResolution.Items.Cast<ComboBoxItem>().FirstOrDefault();
        }

        private void ComboBoxResolution_SelectionChanged(object sender, RoutedEventArgs e) {
            if (ComboBoxResolution.SelectedItem is null) {
                Config.Raw.Width = -1;
                Config.Raw.Height = -1;
                ComboBoxFrameRate.Items.Clear();
                ComboBoxFrameRate.SelectedItem = null;
                ComboBoxFrameRate.IsEnabled = false;
                return;
            }
            var (width, height) = (ValueTuple<int, int>)((ComboBoxItem)ComboBoxResolution.SelectedItem).Tag;
            (Config.Raw.Width, Config.Raw.Height) = (width, height);
            var dev = (MediaCaptureDevice)((ComboBoxItem)ComboBoxCamera.SelectedItem).Tag;
            var framerates = dev.Formats
                .Where(f => f.nWidth == width && f.nHeight == height)
                .Select(f => (double)f.nFrameRateNumerator / f.nFrameRateDenominator)
                .Distinct()//required, subType may be different, with same framerate
                .OrderByDescending(r => r);
            ComboBoxFrameRate.Items.Clear();
            foreach (var framerate in framerates) {
                var name = $"{framerate}";
                var item = new ComboBoxItem() { Content = name, Tag = framerate };
                ComboBoxFrameRate.Items.Add(item);
            }
            ComboBoxFrameRate.IsEnabled = ComboBoxFrameRate.Items.Count > 0;
            foreach (var item in ComboBoxFrameRate.Items.Cast<ComboBoxItem>()) {
                var framerate = (double)item.Tag;
                if (Config.Raw.Framerate != framerate) {
                    continue;
                }
                ComboBoxFrameRate.SelectedItem = item;
                return;
            }
            ComboBoxFrameRate.SelectedItem = ComboBoxFrameRate.Items.Cast<ComboBoxItem>().FirstOrDefault();
        }

        private void ComboBoxFrameRate_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ComboBoxFrameRate.SelectedItem is null) {
                Config.Raw.Framerate = double.NaN;
                return;
            }
            var framerate = (double)((ComboBoxItem)ComboBoxFrameRate.SelectedItem).Tag;
            Config.Raw.Framerate = framerate;
        }
        
    }
}
