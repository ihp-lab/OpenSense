using System.Windows;
using Serilog;

namespace WpfApplication {
    public partial class App : Application {
        private void Application_Startup(object sender, StartupEventArgs e) {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            Log.Fatal("Application unhandled exception: {exception}", e.Exception);
            MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Application_Exit(object sender, ExitEventArgs e) {
            Log.CloseAndFlush();
        }
    }
}
