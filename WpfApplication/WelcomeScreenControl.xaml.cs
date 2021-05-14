using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
namespace OpenSense.Wpf {
    
    public partial class WelcomeScreenControl : UserControl {
        public WelcomeScreenControl() {
            InitializeComponent();

            TextBlockVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static void OpenUrl(string url) {
            var psi = new ProcessStartInfo {
                FileName = url,
                UseShellExecute = true
            };
            try {
                Process.Start(psi);
            } catch (Win32Exception) {

            }
        }

        private void HyperlinkLabWebPage_Click(object sender, RoutedEventArgs e) {
            OpenUrl("https://www.ihp-lab.org");
        }
    }
}
