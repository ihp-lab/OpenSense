using System.Windows;

namespace OpenSense {
    /// <summary>
    /// Application class.
    /// </summary>
    public partial class App : Application {
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show(e.Exception.ToString());
        }
    }
}
