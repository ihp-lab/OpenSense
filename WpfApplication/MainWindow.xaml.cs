using System.Windows;
using OpenSense.WPF.Pipeline;
using OpenSense.WPF.Widget;
using OpenSense.WPF.Widget.Contract;

namespace OpenSense.WPF {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void ButtonPipelineEditor_Click(object sender, RoutedEventArgs e) {
            var editor = new PipelineEditorWindow();
            editor.Show();
        }

        private void ButtonPipelineRunner_Click(object sender, RoutedEventArgs e) {
            var executor = new PipelineRunnerWindow();
            executor.Show();
        }


        #region widgets
        private void ItemsControlWidgets_Initialized(object sender, System.EventArgs e) {
            ItemsControlWidgets.ItemsSource = new WidgetManager().Widgets;
        }

        private void WidgetItem_Click(object sender, RoutedEventArgs e) {
            var fe = (FrameworkElement)sender;
            var widgetMetadata = (IWidgetMetadata)fe.DataContext;
            var win = widgetMetadata.Create();
            win.ShowDialog();
        }
        #endregion


    }
}
