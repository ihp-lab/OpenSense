using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OpenSense.Component.EyePointOfInterest.Common {
    public static class PoiOnDisplayEstimatorHelper {

        public static IPoiOnDisplayEstimator LoadEstimator(string filename) {
            var json = File.ReadAllText(filename);
            var jsonObj = JObject.Parse(json);
            var configTypeString = jsonObj[nameof(PoiOnDisplayEstimatorConfiguration.ConfigurationType)].Value<string>();
            var configType = Type.GetType(configTypeString);
            if (configType is null) {
                throw new Exception($"Estimator configuration type {configTypeString} not found.");
            }
            var configObj = jsonObj.ToObject(configType);
            var config = (PoiOnDisplayEstimatorConfiguration)configObj;
            var estimator = config.Instantiate();
            return estimator;
        }
    }
}
