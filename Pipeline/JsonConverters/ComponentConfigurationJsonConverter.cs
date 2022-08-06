using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Pipeline.JsonConverters {
    internal class ComponentConfigurationJsonConverter : JsonConverter {

        private static string FieldName = "ComponentType";

        public override bool CanConvert(Type objectType) {
            return typeof(ComponentConfiguration).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var assemblyQualifiedName = jsonObject.GetValue(FieldName).ToString();
            var type = Type.GetType(assemblyQualifiedName, true);
            var setting = new JsonSerializerSettings() {
                Converters = serializer.Converters
                    .Where(c => c is not ComponentConfigurationJsonConverter)
                    .ToArray(),
            };
            var subSerializer = JsonSerializer.Create(setting);
            using (var subReader = jsonObject.CreateReader()) {
                return subSerializer.Deserialize(subReader, type);
            }
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object) {
                serializer.Serialize(writer, t);
                return;
            }
            var o = (JObject)t;
            o.AddFirst(new JProperty(FieldName, value.GetType().AssemblyQualifiedName));
            serializer.Serialize(writer, o);
            return;
        }
    }
}
