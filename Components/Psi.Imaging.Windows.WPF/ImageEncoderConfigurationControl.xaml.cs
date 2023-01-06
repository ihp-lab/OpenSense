using System;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Psi.Imaging;

namespace OpenSense.WPF.Components.Psi.Imaging {
    public partial class ImageEncoderConfigurationControl : UserControl {

        private ImageEncoderConfiguration Config => DataContext as ImageEncoderConfiguration;

        public ImageEncoderConfigurationControl() {
            InitializeComponent();
        }

        private void ContentControlEncoder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Config is null) {
                RadioButtonPng.IsChecked = false;
                RadioButtonPng.IsChecked = false;
            } else {
                switch (Config.Encoder) {
                    case PsiBuiltinImageToStreamEncoder.Png:
                        RadioButtonPng.IsChecked = true;
                        break;
                    case PsiBuiltinImageToStreamEncoder.Jpeg:
                        RadioButtonJpeg.IsChecked = true;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        private void RadioButtonEncoder_Checked(object sender, RoutedEventArgs e) {
            var rb = (RadioButton)sender;
            if (rb.IsChecked == false) {
                return;
            }
            if (Config is null) {
                rb.IsChecked = false;
                return;
            }
            Config.Encoder = (PsiBuiltinImageToStreamEncoder)rb.Tag;
        }
    }
}
