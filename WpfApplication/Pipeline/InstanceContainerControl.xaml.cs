using System;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.Contract;

namespace OpenSense.WPF.Pipeline {
    public partial class InstanceContainerControl : UserControl {

        public InstanceContainerControl(string name, UIElement control) {
            InitializeComponent();
            Expander.Header = name;
            Expander.IsExpanded = control != null;
            Expander.IsEnabled = control != null;
            if (control != null) {
                ContentControlDisplay.Children.Add(control);
            }
        }
    }
}
