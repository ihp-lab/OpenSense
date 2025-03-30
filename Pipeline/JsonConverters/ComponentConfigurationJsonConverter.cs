#nullable enable

using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenSense.Components;

namespace OpenSense.Pipeline.JsonConverters {
    internal class ComponentConfigurationJsonConverter : JsonConverter {

        private static string FieldName = "ComponentType";

        /// <remarks>Please add rules in time order, because they will be applied in sequence.</remarks>
        private static readonly TypeRedirectionRule[] RedirectionRules = new TypeRedirectionRule[] {
            /* 2023/10/26 - LibreFace Visualizer assembly name */
            new (
                "OpenSense.Components.LibreFace.Visualizer.ActionUnitIntensityVisualizerConfiguration",
                "OpenSense.WPF.Components.LibreFace",
                null,
                "OpenSense.WPF.Components.LibreFace.Visualizer"
            ),
            new (
                "OpenSense.Components.LibreFace.Visualizer.ActionUnitPresenceVisualizerConfiguration", 
                "OpenSense.WPF.Components.LibreFace",
                null,
                "OpenSense.WPF.Components.LibreFace.Visualizer"
            ),
            new (
                "OpenSense.Components.LibreFace.Visualizer.FacialExpressionVisualizerConfiguration",
                "OpenSense.WPF.Components.LibreFace",
                null,
                "OpenSense.WPF.Components.LibreFace.Visualizer"
            ),
            /* 2025/03/29 - Azure Kinect Visualizer */
            new (
                "OpenSense.Components.AzureKinect.Visualizer.AzureKinectBodyTrackerVisualizerConfiguration",
                "OpenSense.WPF.Components.AzureKinect.Visualizer",
                "OpenSense.Components.Psi.AzureKinect.Visualizer.AzureKinectBodyTrackerVisualizerConfiguration",
                "OpenSense.WPF.Components.Psi.AzureKinect.Visualizer"
            ),
        };

        private static readonly Regex TypeNameRegex = new Regex(@"^(?<type>[^,]+),\s*(?<assembly>[^,]+)(?<rest>.*)$");

        public ComponentConfigurationJsonConverter() {
            
        }

        public override bool CanConvert(Type objectType) {
            return typeof(ComponentConfiguration).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            var jsonObject = JObject.Load(reader);
            var typeField = jsonObject.GetValue(FieldName);
            if (typeField is null) {
                throw new Exception($"Cannot find field {FieldName} in OpenSense component json object.");
            }
            var assemblyQualifiedName = typeField.ToString();
            assemblyQualifiedName = RedirectTypeName(assemblyQualifiedName);
            var type = Type.GetType(assemblyQualifiedName, true);
            var subSerializer = CreateSubSerializer(serializer);
            using (var subReader = jsonObject.CreateReader()) {
                return subSerializer.Deserialize(subReader, type);
            }
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
            JToken t;
            if (value is null) {
                t = JValue.CreateNull();
            } else {
                var subSerializer = CreateSubSerializer(serializer);
                t = JToken.FromObject(value, subSerializer);
            }
            if (t.Type != JTokenType.Object) {
                serializer.Serialize(writer, t);
                return;
            }
            var o = (JObject)t;
            if (value is not null) {
                o.AddFirst(new JProperty(FieldName, value.GetType().AssemblyQualifiedName));
            }
            serializer.Serialize(writer, o);
            return;
        }

        private static JsonSerializer CreateSubSerializer(JsonSerializer serializer) {
            var setting = new JsonSerializerSettings() {
                Converters = serializer.Converters
                    .Where(c => c is not ComponentConfigurationJsonConverter)
                    .ToArray(),
            };
            var result = JsonSerializer.Create(setting);
            return result;
        }

        private static string RedirectTypeName(string assemblyQualifiedName) {
            var match = TypeNameRegex.Match(assemblyQualifiedName);
            if (!match.Success) {
                throw new Exception($"Cannot parse OpenSense component type name {assemblyQualifiedName}.");
            }
            var typeName = match.Groups["type"].Value;
            var assemblyName = match.Groups["assembly"].Value;
            var rest = match.Groups["rest"].Value;

            foreach (var rule in RedirectionRules) {//Apply rule in sequence
                if (rule.OldTypeName != typeName || rule.OldAssemblyName != assemblyName) {
                    continue;
                }
                typeName = rule.NewTypeName ?? typeName;
                assemblyName = rule.NewAssemblyName ?? assemblyName;
            }

            var newName = $"{typeName}, {assemblyName}{rest}";
            string result;
            if (assemblyQualifiedName == newName) {
                result = assemblyQualifiedName;
            } else {
                result = newName;
                Debug.WriteLine($"Redirected OpenSense component type \"{assemblyQualifiedName}\" to \"{newName}\".");
            }
            return result;
        }

        #region Classes
        private sealed class TypeRedirectionRule {

            public string OldTypeName { get; }

            public string OldAssemblyName { get; }

            public string? NewTypeName { get; }

            public string? NewAssemblyName { get; }

            public TypeRedirectionRule(string oldTypeName, string oldAssemblyName, string? newTypeName, string? newAssemblyName) {
                Debug.Assert(!string.IsNullOrEmpty(newTypeName) || !string.IsNullOrEmpty(newAssemblyName));
                OldTypeName = oldTypeName;
                OldAssemblyName = oldAssemblyName;
                NewTypeName = newTypeName;
                NewAssemblyName = newAssemblyName;
            }
        }
        #endregion
    }
}
