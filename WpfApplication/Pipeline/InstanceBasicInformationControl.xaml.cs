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
using OpenSense.Component.Contract;

namespace OpenSense.WPF.Pipeline {
    public partial class InstanceBasicInformationControl : UserControl {

        private ComponentConfiguration Config;

        public InstanceBasicInformationControl(ComponentConfiguration config) {
            InitializeComponent();
            Config = config;
            DataContext = config;

            var metadata = config.GetMetadata();
            TextBlockComponent.Text = metadata.Name;
            TextBlockDescription.Text = metadata.Description;
        }
    }
}
