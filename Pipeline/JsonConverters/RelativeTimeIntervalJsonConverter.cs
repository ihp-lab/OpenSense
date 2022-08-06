using System;
using System.Diagnostics;
using Microsoft.Psi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenSense.Pipeline.JsonConverters {
    internal class RelativeTimeIntervalJsonConverter : JsonConverter {

        public override bool CanConvert(Type objectType) {
            return typeof(RelativeTimeInterval).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonToken = JToken.Load(reader);
            if (jsonToken.Type == JTokenType.Null) {
                return null;
            }
            Debug.Assert(jsonToken.Type == JTokenType.Object);
            var jsonObject = (JObject)jsonToken;
            IntervalEndpoint<TimeSpan> leftEndpoint;
            using (var subReader = jsonObject[nameof(RelativeTimeInterval.LeftEndpoint)].CreateReader()) {
                leftEndpoint = (IntervalEndpoint<TimeSpan>)serializer.Deserialize(subReader, typeof(IntervalEndpoint<TimeSpan>));
            }
            IntervalEndpoint<TimeSpan> rightEndpoint;
            using (var subReader = jsonObject[nameof(RelativeTimeInterval.RightEndpoint)].CreateReader()) {
                rightEndpoint = (IntervalEndpoint<TimeSpan>)serializer.Deserialize(subReader, typeof(IntervalEndpoint<TimeSpan>));
            }
            var result = new RelativeTimeInterval(leftEndpoint, rightEndpoint);
            return result;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
