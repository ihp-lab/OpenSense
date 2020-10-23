using System;
using System.Linq;
using System.Text;
using Microsoft.Psi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSense.PipelineBuilder.JsonConverters {
    internal class DeliveryPolicyJsonConverter : JsonConverter {

        private static readonly JsonSerializer Serializer = new JsonSerializer();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var name = jsonObject[nameof(DeliveryPolicy.Name)].Value<string>();
            switch (name) {
                case nameof(DeliveryPolicy.Unlimited):
                    return DeliveryPolicy.Unlimited;
                case nameof(DeliveryPolicy.LatestMessage):
                    return DeliveryPolicy.LatestMessage;
                case nameof(DeliveryPolicy.Throttle):
                    return DeliveryPolicy.Throttle;
                case nameof(DeliveryPolicy.SynchronousOrThrottle):
                    return DeliveryPolicy.SynchronousOrThrottle;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool CanConvert(Type objectType) {
            return typeof(DeliveryPolicy).IsAssignableFrom(objectType) || typeof(DeliveryPolicy) == objectType;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
