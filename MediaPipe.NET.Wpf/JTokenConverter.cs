#nullable enable

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OpenSense.Wpf.Component.MediaPipe.NET {
    internal sealed class JTokenConverter : IValueConverter {

        private static readonly JsonConverter[] Converters = {
            new Newtonsoft.Json.Converters.StringEnumConverter(new DefaultNamingStrategy(), allowIntegerValues: true),
        };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            Debug.Assert(targetType == typeof(string));
            var token = value as JToken ?? JValue.CreateNull();
            var result = token.ToString(Formatting.Indented, Converters);
            return result;

        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            var json = value as string ?? JValue.CreateNull().ToString();
            var result = JToken.Parse(json);
            return result;
        }
    }
}
