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

namespace OpenSense.PipelineBuilder.Controls {
    public partial class InstanceBasicsControl : UserControl {

        private InstanceConfiguration Config;

        private ComponentDescription Desc => ConfigurationManager.Description(Config);

        public InstanceBasicsControl(InstanceConfiguration config) {
            InitializeComponent();
            Config = config;
            DataContext = config;

            TextBlockComponent.Text = Desc.Name;
            TextBlockDescription.Text = Desc.Description;
        }
    }
}
