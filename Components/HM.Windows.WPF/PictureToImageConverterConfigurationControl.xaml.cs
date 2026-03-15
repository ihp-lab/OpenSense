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
                case nameof(PictureToImageConverterConfiguration.SourceBitDepth):
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
            var sourceBits = config.SourceBitDepth > 0 ? config.SourceBitDepth : 16;
            var scaleShift = config.BitDepthMappingScaleShift;

            // Update preview
            MappingPreview.TargetBitDepth = targetBitDepth;

            // ScaleShift slider
            if (targetBitDepth > 0) {
                ScaleShiftSlider.Minimum = BitDepthMappingInfo.GetMinScaleShift(targetBitDepth);
                ScaleShiftSlider.Maximum = BitDepthMappingInfo.GetMaxScaleShift(sourceBits, targetBitDepth);
            }

            // WindowStart slider
            var step = BitDepthMappingInfo.GetWindowStartStep(scaleShift);
            WindowStartSlider.Minimum = 0;
            WindowStartSlider.Maximum = BitDepthMappingInfo.GetMaxWindowStart(sourceBits, targetBitDepth, scaleShift);
            WindowStartSlider.TickFrequency = step;
            WindowStartSlider.SmallChange = step;
            WindowStartSlider.LargeChange = step;
        }

    }
}
