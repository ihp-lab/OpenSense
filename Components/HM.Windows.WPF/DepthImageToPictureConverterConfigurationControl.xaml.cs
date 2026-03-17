#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class DepthImageToPictureConverterConfigurationControl : UserControl {

        public DepthImageToPictureConverterConfigurationControl() {
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
                case nameof(DepthImageToPictureConverterConfiguration.OutputBitDepth):
                case nameof(DepthImageToPictureConverterConfiguration.BitDepthMappingScaleShift):
                    UpdateSliderRanges();
                    break;
            }
        }

        private void UpdateSliderRanges() {
            if (DataContext is not DepthImageToPictureConverterConfiguration config) {
                return;
            }

            const int sourceBits = 16; // DepthImage is always 16-bit
            var targetBits = config.OutputBitDepth;

            if (targetBits > 0) {
                ScaleShiftSlider.Minimum = BitDepthMappingInfo.GetMinScaleShift(targetBits);
                ScaleShiftSlider.Maximum = BitDepthMappingInfo.GetMaxScaleShift(sourceBits);
            }

            InputStartSlider.Minimum = 0;
            InputStartSlider.Maximum = BitDepthMappingInfo.GetMaxInputStart(sourceBits);

            OutputStartSlider.Minimum = 0;
            OutputStartSlider.Maximum = BitDepthMappingInfo.GetMaxOutputStart(targetBits);
        }
    }
}
