using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Pipeline.JsonConverters {
    internal class ComponentConfigurationJsonConverter : JsonConverter {

        private static string FieldName = "ComponentType";

        private static readonly JsonSerializer Serializer;

        static ComponentConfigurationJsonConverter() {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new DeliveryPolicyJsonConverter());
            Serializer = JsonSerializer.Create(setting);
        }

        public override bool CanConvert(Type objectType) {
            return typeof(ComponentConfiguration).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var assemblyQualifiedName = jsonObject.GetValue(FieldName).ToString();
            var type = Type.GetType(assemblyQualifiedName, true);
            using (var subReader = jsonObject.CreateReader()) {
                return Serializer.Deserialize(subReader, type);
            }
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object) {
                t.WriteTo(writer);
            } else {
                var o = (JObject)t;
                o.AddFirst(new JProperty(FieldName, value.GetType().AssemblyQualifiedName));
                o.WriteTo(writer);
            }
        }
    }
}
