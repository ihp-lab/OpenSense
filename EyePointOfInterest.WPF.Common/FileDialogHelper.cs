using Microsoft.Win32;

namespace OpenSense.WPF.Component.EyePointOfInterest.Common {
    public static class FileDialogHelper {

        public static OpenFileDialog CreateOpenEstimatorConfigurationFileDialog() => new OpenFileDialog {
            CheckFileExists = true,
            AddExtension = true,
            DefaultExt = "*.poi_param.json",
            Filter = "POI on Display Estimator | *.poi_param.json",
        };

        public static SaveFileDialog CreateSaveEstimatorConfigurationFileDialog() => new SaveFileDialog {
            AddExtension = true,
            DefaultExt = "*.poi_param.json",
            Filter = "POI on Display Estimator | *.poi_param.json",
        };

        public static OpenFileDialog CreateOpenEstimatorSampleFileDialog() => new OpenFileDialog {
            CheckFileExists = true,
            AddExtension = true,
            DefaultExt = "*.poi_sample.json",
            Filter = "POI Samples | *.poi_sample.json",
        };

        public static SaveFileDialog CreateSaveEstimatorSampleFileDialog() => new SaveFileDialog {
            AddExtension = true,
            DefaultExt = "*.poi_sample.json",
            Filter = "POI Samples | *.poi_sample.json",
        };
    }
}
