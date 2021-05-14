using System.Windows;
using OpenSense.Wpf.Pipeline;
using OpenSense.Wpf.Widget;
using OpenSense.Wpf.Widget.Contract;

namespace OpenSense.Wpf {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void ButtonPipelineEditor_Click(object sender, RoutedEventArgs e) {
            var editor = new PipelineEditorWindow();
            editor.Show();
        }

        private void ButtonPipelineExecuter_Click(object sender, RoutedEventArgs e) {
            var executor = new PipelineExecuterWindow();
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
