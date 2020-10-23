using System;
using System.Linq;
using System.Text;
using Microsoft.Psi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OpenSense.PipelineBuilder.JsonConverters {
    internal class InstanceConfigurationJsonConverter : JsonConverter {

        private static readonly JsonSerializer Serializer;

        static InstanceConfigurationJsonConverter() {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new DeliveryPolicyJsonConverter());
            Serializer = JsonSerializer.Create(setting);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var assemblyQualifiedName = jsonObject.GetValue(nameof(InstanceConfiguration.Type)).ToString();
            var type = Type.GetType(assemblyQualifiedName, true);
            using (var subReader = jsonObject.CreateReader()) {
                return Serializer.Deserialize(subReader, type);
            }
        }

        public override bool CanConvert(Type objectType) {
            return typeof(InstanceConfiguration).IsAssignableFrom(objectType) || typeof(InstanceConfiguration) == objectType;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
