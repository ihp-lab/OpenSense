using System;
using System.IO;
using System.Text.Json.Nodes;

namespace OpenSense.Components.EyePointOfInterest.Common {
    public static class PoiOnDisplayEstimatorHelper {

        public static IPoiOnDisplayEstimator LoadEstimator(string filename) {//Note: Not tested after moving from Json.Net to System.Text.Json
            var json = File.ReadAllText(filename);
            var jsonObj = JsonNode.Parse(json);
            var configTypeString = jsonObj[nameof(PoiOnDisplayEstimatorConfiguration.ConfigurationType)].GetValue<string>();
            var configType = Type.GetType(configTypeString);
            if (configType is null) {
                throw new Exception($"Estimator configuration type {configTypeString} not found.");
            }
            var config = jsonObj.GetValue<PoiOnDisplayEstimatorConfiguration>();
            var estimator = config.Instantiate();
            return estimator;
        }
    }
}
