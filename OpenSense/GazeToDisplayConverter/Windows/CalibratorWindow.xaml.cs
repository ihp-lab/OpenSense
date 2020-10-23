using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using MathNet.Spatial.Euclidean;
using Newtonsoft.Json;
using OpenSense.DataStructure;
using OpenSense.Utilities;

namespace OpenSense.GazeToDisplayConverter {
    public partial class CalibratorWindow : Window {
        public CalibratorWindow() {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            TabItemRegression.DataContext = new ObservableCollection<Record>();
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
                        ((ObservableCollection<Record>)TabItemRegression.DataContext).Add((Record)dataPoint);
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
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.gaze_sample.json",
                Filter = "Gaze Samples | *.gaze_sample.json",
            };
            if (saveFileDialog.ShowDialog() == true) {
                var json = JsonConvert.SerializeObject((ObservableCollection<Record>)TabItemRegression.DataContext);
                File.WriteAllText(saveFileDialog.FileName, json);
            }
        }

        private void ButtonLoadDataPoints_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.gaze_sample.json",
                Filter = "Gaze Samples | *.gaze_sample.json",
            };
            if (openFileDialog.ShowDialog() == true) {
                var json = File.ReadAllText(openFileDialog.FileName);
                TabItemRegression.DataContext = JsonConvert.DeserializeObject<ObservableCollection<Record>>(json);//TODO: rewrite deserialization
            }
        }

        private void ButtonClearDataPoints_Click(object sender, RoutedEventArgs e) {
            ((ObservableCollection<Record>)TabItemRegression.DataContext).Clear();
        }
        
        private bool TrySaveModel(IGazeToDisplayConverter converter) {
            if (CheckBoxSaveConverter.IsChecked != true) {
                return false;
            }
            if (string.IsNullOrWhiteSpace(TextBoxConverterFilename.Text) ) {
                MessageBox.Show("Illegal filename");
                return false;
            }
            var param = converter.Save();
            var json = JsonConvert.SerializeObject(param);
            var path = TextBoxConverterFilename.Text.Trim();
            File.WriteAllText(path, json);
            return true;
        }

        private void ButtonRegression_Click(object sender, RoutedEventArgs e) {
            var data = ((ObservableCollection<Record>)TabItemRegression.DataContext).ToList();
            if (data is null || data.Count == 0) {
                MessageBox.Show("No sample data");
                return;
            }
            IGazeToDisplayConverter converter;
            switch (ComboBoxTrainModelType.SelectedIndex) {
                case 0:
                    converter = new TwoStageConverter();
                    var twoStageConverter = (TwoStageConverter)converter;
                    twoStageConverter.Order = int.Parse(TextBoxPolyOrder.Text);
                    break;
                case 1:
                    converter = new EndToEndConverter();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            var (rSquaredX, rSquaredY) = converter.Train(data);
            TabItemTest.DataContext = converter;
            TrySaveModel(converter);
            MessageBox.Show($"Finished:\nR-Squared ({rSquaredX}, {rSquaredX})");
        }

        private void ButtonConverterFilename_Click(object sender, RoutedEventArgs e) {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog {
                AddExtension = true,
                DefaultExt = "*.gaze_conv.json",
                Filter = "Gaze Converter | *.gaze_conv.json",
            };
            saveFileDialog.FileName = TextBoxConverterFilename.Text.Trim();
            if (saveFileDialog.ShowDialog() == true) {
                TextBoxConverterFilename.Text = saveFileDialog.FileName;
            }
        }
        #endregion
        #region Test
        private void ButtonLoadModel_Click(object sender, RoutedEventArgs e) {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog {
                CheckFileExists = true,
                AddExtension = true,
                DefaultExt = "*.json",
                Filter = "JSON | *.json",
            };
            if (openFileDialog.ShowDialog() != true) {
                return;
            }
            var converter = GazeToDisplayConverterHelper.Load(openFileDialog.FileName);
            TabItemTest.DataContext = converter;
        }

        private void ButtonPredict_Click(object sender, RoutedEventArgs e) {
            if (!(TabItemTest.DataContext is IGazeToDisplayConverter converter)) {
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
                Converter = converter,
            };
            predictWin.Owner = this;
            predictWin.ShowDialog();
        }

        #endregion


    }
}
