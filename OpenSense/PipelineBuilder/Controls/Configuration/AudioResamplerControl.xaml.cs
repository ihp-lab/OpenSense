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
    public partial class AudioResamplerControl : UserControl {

        private AudioResamplerConfiguration Config => DataContext as AudioResamplerConfiguration;

        public AudioResamplerControl() {
            InitializeComponent();
        }
        /*
        private void ContentControlInputFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlInputFormat.Children.Clear();
            if (Config != null) {
                var control = new WaveFormatControl(Config.Raw.InputFormat);
                ContentControlInputFormat.Children.Add(control);
            }
        }
        */
        private void ContentControlOutputFormat_Loaded(object sender, RoutedEventArgs e) {
            ContentControlOutputFormat.Children.Clear();
            if (Config != null) {
                var control = new WaveFormatControl(Config.Raw.OutputFormat);
                ContentControlOutputFormat.Children.Add(control);
            }
        }
    }
}
