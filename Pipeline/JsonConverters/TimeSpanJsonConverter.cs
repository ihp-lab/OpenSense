using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSense.Pipeline.JsonConverters {
    internal class TimeSpanJsonConverter : JsonConverter {

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonToken = JToken.Load(reader);
            TimeSpan result;
            switch (jsonToken.Type) {
                case JTokenType.Integer:
                    /** All TimeSpans in new configurations are saved as ticks
                     */
                    var ticks = jsonToken.Value<long>();
                    result = new TimeSpan(ticks);
                    break;
                case JTokenType.String:
                    /** Old configurations has string representations.
                     */
                    var str = jsonToken.Value<string>();
                    result = TimeSpan.Parse(str);
                    break;
                default:
                    throw new InvalidOperationException($"This TimeSpan JSON converter does not support converting JSON token of type {Enum.GetName(typeof(JTokenType), jsonToken.Type)} to TimeSpan.");
            }            
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
