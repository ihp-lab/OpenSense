using System;
using System.Diagnostics;
using Microsoft.Psi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSense.Pipeline.JsonConverters {
    internal class IntervalEndpointJsonConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(IntervalEndpoint<TimeSpan>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonToken = JToken.Load(reader);
            if (jsonToken.Type == JTokenType.Null) {
                return null;
            }
            Debug.Assert(jsonToken.Type == JTokenType.Object);
            var jsonObject = (JObject)jsonToken;
            IntervalEndpoint<TimeSpan> result;
            var bounded = jsonObject[nameof(IntervalEndpoint<TimeSpan>.Bounded)].Value<bool>();
            TimeSpan point;
            using (var subReader = jsonObject[nameof(IntervalEndpoint<TimeSpan>.Point)].CreateReader()) {
                point = (TimeSpan)serializer.Deserialize(subReader, typeof(TimeSpan));
            }
            if (bounded) {
                var inclusive = jsonObject[nameof(IntervalEndpoint<TimeSpan>.Inclusive)].Value<bool>();
                result = new IntervalEndpoint<TimeSpan>(point, inclusive);
            } else {
                result = new IntervalEndpoint<TimeSpan>(point);
            }
            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
