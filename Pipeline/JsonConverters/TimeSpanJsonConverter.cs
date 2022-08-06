using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSense.Pipeline.JsonConverters {
    internal class TimeSpanJsonConverter : JsonConverter {

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonToken = JToken.Load(reader);
            Debug.Assert(jsonToken.Type == JTokenType.Integer);
            var ticks = jsonToken.Value<long>();
            var result = new TimeSpan(ticks);
            return result;
        }

        public override bool CanConvert(Type objectType) {
            return typeof(TimeSpan).IsAssignableFrom(objectType);
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var timeSpan = (TimeSpan)value;
            var ticks = timeSpan.Ticks;
            writer.WriteValue(ticks);
        }
    }
}
