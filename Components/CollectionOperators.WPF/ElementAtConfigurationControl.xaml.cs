using System.Windows.Controls;
using OpenSense.Components.CollectionOperators;

namespace OpenSense.WPF.Components.CollectionOperators {
    public partial class ElementAtConfigurationControl : UserControl {

        private ElementAtConfiguration Config => DataContext as ElementAtConfiguration;

        public ElementAtConfigurationControl() {
            InitializeComponent();
        }
    }
}
