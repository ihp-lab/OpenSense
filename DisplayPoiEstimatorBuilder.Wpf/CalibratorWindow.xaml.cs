using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;
using OpenSense.Component.EyePointOfInterest.Common;
using OpenSense.Component.EyePointOfInterest.Regression;
using OpenSense.Component.EyePointOfInterest.SpatialTracking;
using OpenSense.Wpf.Component.EyePointOfInterest.Common;

namespace OpenSense.Wpf.Widget.DisplayPoiEstimatorBuilder {
    public partial class CalibratorWindow : Window {
        public CalibratorWindow() {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            TabItemRegression.DataContext = new ObservableCollection<GazeToDisplayCoordinateMappingRecord>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            var source = PresentationSource.FromVisual(this);
            if (source != null) {
                var pixelPerMmX = 96.0 * source.CompositionTarget.TransformToDevice.M11 / 25.4;
                var pixelPerMmY = 96.0 * source.CompositionTarget.TransformToDevice.M22 / 25.4;
                var physicalWidth = (float)(SystemParameters.PrimaryScreenWidth / pixelPerMmX);
                var physicalHeight = (float)(SystemParameters.PrimaryScreenHeight / pixelPerMmY);
                /*
                TextBoxScreenWidth.Text = physicalWidth.ToString();
                TextBoxScreenHeight.Text = physicalHeight.ToString();
                TextBoxScreenOffsetX.Text = (-physicalWidth / 2).ToString();
                TextBoxScreenOffsetY.Text = "0";
                TextBoxScreenOffsetZ.Text = "0";
                TextBoxScreenPitch.Text = "0";
                TextBoxScreenYaw.Text = "0";
                TextBoxScreenRoll.Text = "0";
                */
            }
        }

        #region capture

        private void ButtonCapture_Click(object sender, RoutedEventArgs e) {
            if (!int.TryParse(TextBoxDuration.Text, out var seconds) || seconds <= 0) {
                MessageBox.Show("Illegal duration");
                return;
            }
            var resolution = (Resolution)ComboBoxResolution.SelectedItem;
            var calibWin = new CaptureWindow() {
                Duration = TimeSpan.FromSeconds(seconds),
                FlipX = CheckBoxFlipX.IsChecked == true, FlipY = CheckBoxFlipY.IsChecked == true,
                WebcamSymbolicLink = (ComboBoxWebcam.SelectedItem as VideoDevice)?.Name ?? string.Empty,
                WebcamWidth = resolution.Width,
                WebcamHeight = resolution.Height,
                WebcamFx = float.Parse(TextBoxCamFx.Text.Trim()),
                WebcamFy = float.Parse(TextBoxCamFy.Text.Trim()),
                WebcamCx = float.Parse(TextBoxCamCx.Text.Trim()),
                WebcamCy = float.Parse(TextBoxCamCy.Text.Trim()),
            };
            calibWin.Owner = this;
            var result = calibWin.ShowDialog();
            switch (result) {
                default:
                    MessageBox.Show("Canceled");
                    break;
                case true:
                    var data = calibWin.DataPoints.ToList();
                    if (data.Count == 0) {
                        MessageBox.Show("Empty result");
                        return;
                    }
                    foreach (var dataPoint in calibWin.DataPoints) {
                        ((ObservableCollection<GazeToDisplayCoordinateMappingRecord>)TabItemRegression.DataContext).Add((GazeToDisplayCoordinateMappingRecord)dataPoint);
                    }
                    MessageBox.Show($"Finished:\n{calibWin.DataPoints.Count} data points added");
                    break;
            }
        }

        private void ComboBoxWebcam_Loaded(object sender, RoutedEventArgs e) {
            ComboBoxWebcam.ItemsSource = VideoDevice.Devices();
            ComboBoxWebcam.DisplayMemberPath = "Name";
            ComboBoxWebcam.SelectedIndex = 0;
            
        }
        private void ComboBoxWebcam_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            ComboBoxResolution.SelectedIndex = 0;
        }
        #endregion
        #region Train
        private void ButtonSaveDataPoints_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = FileDialogHelper.CreateSaveEstimatorSampleFileDialog();
            if (saveFileDialog.ShowDialog() == true) {
                var json = JsonConvert.SerializeObject((ObservableCollection<GazeToDisplayCoordinateMappingRecord>)TabItemRegression.DataContext);
                File.WriteAllText(saveFileDialog.FileName, json);
            }
        }

        private void ButtonLoadDataPoints_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = FileDialogHelper.CreateOpenEstimatorSampleFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                var json = File.ReadAllText(openFileDialog.FileName);
                TabItemRegression.DataContext = JsonConvert.DeserializeObject<ObservableCollection<GazeToDisplayCoordinateMappingRecord>>(json);//TODO: rewrite deserialization
            }
        }

        private void ButtonClearDataPoints_Click(object sender, RoutedEventArgs e) {
            ((ObservableCollection<GazeToDisplayCoordinateMappingRecord>)TabItemRegression.DataContext).Clear();
        }
        
        private bool TrySaveModel(IPoiOnDisplayEstimator estimator) {
            if (CheckBoxSaveEstimator.IsChecked != true) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(TextBoxEstimatorFilename.Text) ) {
                MessageBox.Show("Illegal filename");
                return false;
            }
            var param = estimator.Save();
            var json = JsonConvert.SerializeObject(param);
            var path = TextBoxEstimatorFilename.Text.Trim();
            File.WriteAllText(path, json);
            return true;
        }

        private void ButtonRegression_Click(object sender, RoutedEventArgs e) {
            var data = ((ObservableCollection<GazeToDisplayCoordinateMappingRecord>)TabItemRegression.DataContext).ToList();
            if (data is null || data.Count == 0) {
                MessageBox.Show("No sample data");
                return;
            }
            IPoiOnDisplayEstimator estimator;
            switch (ComboBoxTrainModelType.SelectedIndex) {
                case 0:
                    estimator = new RegressionPoiOnDisplayEstimator();
                    break;
                case 1:
                    estimator = new SpatialTrackingPoiOnDisplayEstimator();
                    var stEstimator = (SpatialTrackingPoiOnDisplayEstimator)estimator;
                    stEstimator.Order = int.Parse(TextBoxPolyOrder.Text);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            var rSquared = estimator.Train(data);
            TabItemTest.DataContext = estimator;
            TrySaveModel(estimator);
            MessageBox.Show($"Finished:\nR-Squared ({rSquared.X}, {rSquared.Y})");
        }

        private void ButtonEstimatorFilename_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = FileDialogHelper.CreateSaveEstimatorConfigurationFileDialog();
            saveFileDialog.FileName = TextBoxEstimatorFilename.Text.Trim();
            if (saveFileDialog.ShowDialog() == true) {
                TextBoxEstimatorFilename.Text = saveFileDialog.FileName;
            }
        }
        #endregion
        #region Test
        private void ButtonLoadModel_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = FileDialogHelper.CreateOpenEstimatorConfigurationFileDialog();
            if (openFileDialog.ShowDialog() != true) {
                return;
            }
            var converter = PoiOnDisplayEstimatorHelper.LoadEstimator(openFileDialog.FileName);
            TabItemTest.DataContext = converter;
        }

        private void ButtonPredict_Click(object sender, RoutedEventArgs e) {
            if (!(TabItemTest.DataContext is IPoiOnDisplayEstimator estimator)) {
                MessageBox.Show("No active converter");
                return;
            }
            var resolution = (Resolution)ComboBoxResolution.SelectedItem;
            var predictWin = new PredictionWindow() {
                FlipX = CheckBoxFlipX.IsChecked == true,
                FlipY = CheckBoxFlipY.IsChecked == true,
                WebcamSymbolicLink = (ComboBoxWebcam.SelectedItem as VideoDevice)?.Name ?? string.Empty,
                WebcamWidth = resolution.Width,
                WebcamHeight = resolution.Height,
                WebcamFx = float.Parse(TextBoxCamFx.Text.Trim()),
                WebcamFy = float.Parse(TextBoxCamFy.Text.Trim()),
                WebcamCx = float.Parse(TextBoxCamCx.Text.Trim()),
                WebcamCy = float.Parse(TextBoxCamCy.Text.Trim()),
                Estimator = estimator,
            };
            predictWin.Owner = this;
            predictWin.ShowDialog();
        }

        #endregion


    }
}
