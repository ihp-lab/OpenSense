using System.Windows.Controls;
using OpenSense.Component.PythonSupports;

namespace OpenSense.WPF.Component.PythonSupports {
    public partial class PythonConfigurationControl : UserControl {

        private PythonConfiguration Conf => DataContext as PythonConfiguration;

        public PythonConfigurationControl() {
            InitializeComponent();
        }
    }
}
