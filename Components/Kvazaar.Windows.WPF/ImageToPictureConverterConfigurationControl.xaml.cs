#nullable enable

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using KvazaarInterop;
using OpenSense.Components.Kvazaar;

namespace OpenSense.WPF.Components.Kvazaar {
    public sealed partial class ImageToPictureConverterConfigurationControl : UserControl {

        public ImageToPictureConverterConfigurationControl() {
            InitializeComponent();
#if FIXED_BIT_DEPTH
            OutputBitDepthLabel.Visibility = Visibility.Collapsed;
            OutputBitDepthControl.Visibility = Visibility.Collapsed;
#else
            OutputBitDepthSlider.SetBinding(Slider.ValueProperty, new Binding(nameof(ImageToPictureConverterConfiguration.OutputBitDepth)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, });
            OutputBitDepthTextBox.SetBinding(TextBox.TextProperty, new Binding(nameof(ImageToPictureConverterConfiguration.OutputBitDepth)) { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, });
#endif
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
#if !FIXED_BIT_DEPTH
                case nameof(ImageToPictureConverterConfiguration.OutputBitDepth):
#endif
                case nameof(ImageToPictureConverterConfiguration.InputBitDepth):
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
#if FIXED_BIT_DEPTH
            var targetBits = Picture.MaxBitDepth;
#else
            var targetBits = config.OutputBitDepth;
#endif

            // ScaleShift slider
            if (targetBits > 0) {
                ScaleShiftSlider.Minimum = BitDepthMappingInfo.GetMinScaleShift(targetBits);
                ScaleShiftSlider.Maximum = BitDepthMappingInfo.GetMaxScaleShift(sourceBits);
            }

            // InputStart slider
            InputStartSlider.Minimum = 0;
            InputStartSlider.Maximum = BitDepthMappingInfo.GetMaxInputStart(sourceBits);

            // OutputStart slider
            OutputStartSlider.Minimum = 0;
            OutputStartSlider.Maximum = BitDepthMappingInfo.GetMaxOutputStart(targetBits);
        }
    }
}
