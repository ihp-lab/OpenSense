using System;
using System.Windows;
using System.Windows.Controls;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.Components.EyePointOfInterest.Common;
using OpenSense.WPF.Components.EyePointOfInterest.Common;

namespace OpenSense.WPF.Components.EyePointOfInterest {
    public partial class DisplayPoiEstimatorInstanceControl : UserControl {
        private DisplayPoiEstimator Component => DataContext as DisplayPoiEstimator;

        public DisplayPoiEstimatorInstanceControl() {
            InitializeComponent();
        }

        private void ButtonSetEstimator_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = FileDialogHelper.CreateOpenEstimatorConfigurationFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                try {
                    Component.Estimator = PoiOnDisplayEstimatorHelper.LoadEstimator(openFileDialog.FileName);
                    TextBlockEstimatorNotification.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Loaded");
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString(), "Failed to load estimator", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
