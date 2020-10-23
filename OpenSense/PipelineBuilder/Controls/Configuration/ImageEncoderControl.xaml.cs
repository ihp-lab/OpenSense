using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace OpenSense.PipelineBuilder.Controls.Configuration {
    public partial class ImageEncoderControl : UserControl {

        private ImageEncoderConfiguration Config => DataContext as ImageEncoderConfiguration;

        public ImageEncoderControl() {
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
