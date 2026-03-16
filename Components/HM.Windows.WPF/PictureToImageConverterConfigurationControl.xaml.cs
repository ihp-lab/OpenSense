#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.HM;

namespace OpenSense.WPF.Components.HM {
    public sealed partial class PictureToImageConverterConfigurationControl : UserControl {

        public PictureToImageConverterConfigurationControl() {
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
            UpdateDerivedValues();
        }

        private void OnConfigPropertyChanged(object? sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(PictureToImageConverterConfiguration.OutputPixelFormat):
                case nameof(PictureToImageConverterConfiguration.InputBitDepth):
                case nameof(PictureToImageConverterConfiguration.BitDepthMappingScaleShift):
                    UpdateDerivedValues();
                    break;
            }
        }

        private void UpdateDerivedValues() {
            if (DataContext is not PictureToImageConverterConfiguration config) {
                return;
            }

            var targetBitDepth = PixelFormatInfo.GetBitDepth(config.OutputPixelFormat);
            var sourceBits = config.InputBitDepth ?? 16;
            var scaleShift = config.BitDepthMappingScaleShift;

            // Update preview
            MappingPreview.TargetBitDepth = targetBitDepth;

            // ScaleShift slider
            if (targetBitDepth > 0) {
                ScaleShiftSlider.Minimum = BitDepthMappingInfo.GetMinScaleShift(targetBitDepth);
                ScaleShiftSlider.Maximum = BitDepthMappingInfo.GetMaxScaleShift(sourceBits);
            }

            // InputStart slider
            InputStartSlider.Minimum = 0;
            InputStartSlider.Maximum = BitDepthMappingInfo.GetMaxInputStart(sourceBits);

            // OutputStart slider
            OutputStartSlider.Minimum = 0;
            OutputStartSlider.Maximum = BitDepthMappingInfo.GetMaxOutputStart(targetBitDepth);
        }

    }
}
