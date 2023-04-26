using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Psi;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenSense.Components.Contract;
using OpenSense.Pipeline.JsonConverters;

namespace OpenSense.Pipeline {
    [Serializable]
    public class PipelineConfiguration : INotifyPropertyChanged {

        private static IList<JsonConverter> DefaultConverters = new JsonConverter[] {
            new Newtonsoft.Json.Converters.StringEnumConverter(new DefaultNamingStrategy(), allowIntegerValues: true),
            new ComponentConfigurationJsonConverter(),
            new TimeSpanJsonConverter(),
            new RelativeTimeIntervalJsonConverter(),
            new IntervalEndpointJsonConverter(),
            new DeliveryPolicyJsonConverter(),
        };

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private Guid id = Guid.NewGuid();

        public Guid Id {
            get => id;
            set => SetProperty(ref id, value);
        }

        private string name = string.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string Name {
            get => name;
            set => SetProperty(ref name, value);
        }

        private string description = string.Empty;

        /// <summary>
        /// Display description. Can be used as a memo field.
        /// </summary>
        public string Description {
            get => description;
            set => SetProperty(ref description, value);
        }

        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage; // to prevent memory overflow

        /// <summary>
        /// Pipeline default delivery policy.
        /// </summary>
        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        private ObservableCollection<ComponentConfiguration> instances = new ObservableCollection<ComponentConfiguration>();

        public ObservableCollection<ComponentConfiguration> Instances {
            get => instances;
            set => SetProperty(ref instances, value);
        }

        public PipelineConfiguration() {

        }

        public PipelineConfiguration(string json/*, params JsonConverter[] extraConverters*/): this() {
            var setting = new JsonSerializerSettings() {
                Converters = DefaultConverters,
            };
            JsonConvert.PopulateObject(json, this, setting);
        }

        public string ToJson() {
            var jsonSettings = new JsonSerializerSettings() {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = DefaultConverters,
            };
            var jsonSerializer = JsonSerializer.Create(jsonSettings);
            using (var stringWriter = new StringWriter()) {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter) {
                    IndentChar = '\t',
                    Indentation = 1,
                    QuoteName = false,
                    Formatting = Formatting.Indented,
                }) {
                    jsonSerializer.Serialize(jsonTextWriter, this);
                }
                return stringWriter.ToString();
            }
        }
    }
}
