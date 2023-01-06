using System.Windows.Controls;
using OpenSense.Components.PythonSupports;

namespace OpenSense.WPF.Components.PythonSupports {
    public partial class PythonConfigurationControl : UserControl {

        private PythonConfiguration Conf => DataContext as PythonConfiguration;

        public PythonConfigurationControl() {
            InitializeComponent();
        }
    }
}
