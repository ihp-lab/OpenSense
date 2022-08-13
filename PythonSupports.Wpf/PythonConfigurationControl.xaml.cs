using System.Windows.Controls;
using OpenSense.Component.PythonSupports;

namespace OpenSense.Wpf.Component.PythonSupports {
    public partial class PythonConfigurationControl : UserControl {

        private PythonConfiguration Conf => DataContext as PythonConfiguration;

        public PythonConfigurationControl() {
            InitializeComponent();
        }
    }
}
