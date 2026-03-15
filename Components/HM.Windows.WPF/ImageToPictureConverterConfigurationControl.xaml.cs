#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class ImageToPictureConverterConfigurationControl : UserControl {

        public ImageToPictureConverterConfigurationControl() {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is INotifyPropertyChanged oldVm) {
                oldVm.PropertyChanged -= OnConfigPropertyChanged;
            }
            if (e.NewValue is INotifyPropertyChanged newVm) {
                newVm.PropertyChanged += OnConfigPropertyChanged;
            }
            UpdateSliderRanges();
        }

        private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(ImageToPictureConverterConfiguration.InputBitDepth):
                case nameof(ImageToPictureConverterConfiguration.OutputBitDepth):
                case nameof(ImageToPictureConverterConfiguration.BitDepthMappingScaleShift):
                    UpdateSliderRanges();
                    break;
            }
        }

        private void UpdateSliderRanges() {
            if (DataContext is not ImageToPictureConverterConfiguration config) {
                return;
            }

            var sourceBits = config.InputBitDepth ?? 16;
            var targetBits = config.OutputBitDepth;
            var scaleShift = config.BitDepthMappingScaleShift;

            // ScaleShift slider
            if (targetBits > 0) {
                ScaleShiftSlider.Minimum = BitDepthMappingInfo.GetMinScaleShift(targetBits);
                ScaleShiftSlider.Maximum = BitDepthMappingInfo.GetMaxScaleShift(sourceBits, targetBits);
            }

            // WindowStart slider
            var step = BitDepthMappingInfo.GetWindowStartStep(scaleShift);
            WindowStartSlider.Minimum = 0;
            WindowStartSlider.Maximum = BitDepthMappingInfo.GetMaxWindowStart(sourceBits, targetBits, scaleShift);
            WindowStartSlider.TickFrequency = step;
            WindowStartSlider.SmallChange = step;
            WindowStartSlider.LargeChange = step;
        }
    }
}
