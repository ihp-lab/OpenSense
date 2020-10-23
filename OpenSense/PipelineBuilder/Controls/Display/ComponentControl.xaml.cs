using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenSense.Components.Display;

namespace OpenSense.PipelineBuilder.Controls.Display {
    public partial class ComponentControl : UserControl {
        public ComponentControl(InstanceEnvironment env) {
            InitializeComponent();
            DataContext = env;
            var control = ControlFactory.CreateVisualizerControl(env);
            if (control != null) {
                control.DataContext = env.Instance;
                Expander.IsExpanded = true;
                ContentControlDisplay.Children.Add(control);
            }
            
        }
    }
}
