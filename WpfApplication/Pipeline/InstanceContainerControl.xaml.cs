using System;
using System.Windows;
using System.Windows.Controls;

namespace OpenSense.WPF.Pipeline {
    public partial class InstanceContainerControl : UserControl {

        public InstanceContainerControl() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var tuple = DataContext as Tuple<string, object, UIElement>;
            if (tuple is null) {
                ContentControlDisplay.Content = null;
                Expander.Header = "";
                Expander.IsExpanded = false;
                Expander.IsEnabled = false;
                return;
            }
            Expander.Header = tuple.Item1;
            Expander.IsExpanded = true;
            Expander.IsEnabled = true;
            ContentControlDisplay.DataContext = tuple.Item2;
            ContentControlDisplay.Content = tuple.Item3;
        }
    }
}
