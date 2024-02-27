using System;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using OpenSense.Components.EyePointOfInterest;
using OpenSense.Components.EyePointOfInterest.Regression;
using OpenSense.Components.EyePointOfInterest.SpatialTracking;

namespace OpenSense.WPF.Widgets.DisplayPoiEstimatorBuilder {
    internal sealed class EstimatorConfigurationTypeResolver : DefaultJsonTypeInfoResolver {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options) {
            var jsonTypeInfo = base.GetTypeInfo(type, options);
            var baseType = typeof(PoiOnDisplayEstimatorConfiguration);
            if (jsonTypeInfo.Type == baseType) {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions {
                    TypeDiscriminatorPropertyName = PoiOnDisplayEstimatorConfiguration.TypeDiscriminatorPropertyName,
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes = {
                        new JsonDerivedType(typeof(RegressionPoiOnDisplayEstimatorConfiguration), typeof(RegressionPoiOnDisplayEstimatorConfiguration).AssemblyQualifiedName),
                        new JsonDerivedType(typeof(SpatialTrackingPoiOnDisplayEstimatorConfiguration), typeof(SpatialTrackingPoiOnDisplayEstimatorConfiguration).AssemblyQualifiedName),
                    }
                };
            }
            return jsonTypeInfo;
        }
    }
}
