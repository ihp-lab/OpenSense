#nullable enable

using System;
using System.Globalization;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSense.Components.MediaPipe.NET;

namespace OpenSense.WPF.Components.MediaPipe.NET {
    internal sealed class JTokenRule : ValidationRule {

        public PacketType? PacketType { get; set; }

        public override ValidationResult Validate(object? value, CultureInfo cultureInfo) {
            var json = value as string ?? JValue.CreateNull().ToString();
            JToken token;
            try {
                token = JToken.Parse(json);
            } catch (JsonException ex) {
                return new ValidationResult(isValid: false, ex.Message);
            }
            if (PacketType is not null) {
                var jValue = token as JValue;
                switch (PacketType) {
                    case OpenSense.Components.MediaPipe.NET.PacketType.Bool:
                        if (jValue is null || jValue.Type != JTokenType.Boolean) {
                            return new ValidationResult(isValid: false, "JSON value is not a Boolean.");
                        }
                        break;
                    case OpenSense.Components.MediaPipe.NET.PacketType.Int:
                        if (jValue is null || jValue.Type != JTokenType.Integer) {
                            return new ValidationResult(isValid: false, "JSON value is not a Integer.");
                        }
                        break;
                    //TODO: Add more
                    default:
                        throw new NotImplementedException();
                }
            }
            return new ValidationResult(isValid: true, null);
        }
    }
}
