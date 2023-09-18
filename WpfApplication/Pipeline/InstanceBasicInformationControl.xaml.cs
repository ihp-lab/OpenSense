using System.Windows.Controls;
using OpenSense.Components;

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
