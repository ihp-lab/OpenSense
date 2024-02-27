using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenSense.Components.EyePointOfInterest {
    public static class PoiOnDisplayEstimatorHelper {

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions() {
            IncludeFields = true,//Needed for Vector2 and Vector3
        };

        public static IPoiOnDisplayEstimator LoadEstimator(string filename) {
            var json = File.ReadAllText(filename);
            var jsonObj = JsonNode.Parse(json);
            var prop = jsonObj[PoiOnDisplayEstimatorConfiguration.TypeDiscriminatorPropertyName];
            if (prop is null) {
                throw new Exception($"Estimator configuration type discriminator {PoiOnDisplayEstimatorConfiguration.TypeDiscriminatorPropertyName} not found.");
            }
            var configTypeString = prop.GetValue<string>();
            var configType = Type.GetType(configTypeString);
            if (configType is null) {
                throw new Exception($"Estimator configuration type {configTypeString} not found.");
            }
            var config = (PoiOnDisplayEstimatorConfiguration)jsonObj.Deserialize(configType, SerializerOptions);
            var estimator = config.Instantiate();
            return estimator;
        }
    }
}
