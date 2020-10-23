using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Data;
using Microsoft.Psi.Remoting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OpenSense.PipelineBuilder.Configurations;
using OpenSense.PipelineBuilder.JsonConverters;

namespace OpenSense.PipelineBuilder {
    public static class ConfigurationManager {

        public static IList<ComponentDescription> Components => new List<ComponentDescription>() {
            #region Psi
            LocalExporterConfiguration.Description,
            RemoteExporterConfiguration.Description,
            StreamWriterImplConfiguration.Description,
            LocalImporterConfiguration.Description,
            RemoteImporterConfiguration.Description,
            StreamReaderImplConfiguration.Description,
            JoinOperatorConfiguration.Description,

            MediaCaptureConfiguration.Description,
            AudioCaptureConfiguration.Description,
            AzureKinectSensorConfiguration.Description,
            MediaSourceConfiguration.Description,
            WaveFileAudioSourceConfiguration.Description,
#if NETFRAMEWORK
            SystemVoiceActivityDetectorConfiguration.Description,
            SystemSpeechRecognizerConfiguration.Description,
            KinectSensorConfiguration.Description,
#endif
            AudioResamplerConfiguration.Description,
            AzureSpeechRecognizerConfiguration.Description,
            AzureKinectBodyTrackerConfiguration.Description,
            Mpeg4WriterConfiguration.Description,
            WaveFileWriterConfiguration.Description,
            AudioPlayerConfiguration.Description,
            ImageEncoderConfiguration.Description,
            ImageDecoderConfiguration.Description,
            #endregion

            BiopacConfiguration.Description,

            FlipColorVideoConfiguration.Description,
            GazeToDisplayConfiguration.Description,
            OpenPoseConfiguration.Description,
            OpenFaceConfiguration.Description,
            OpenSmileConfiguration.Description,
            HeadGestureDetectorConfiguration.Description,
            EmotionDetectorConfiguration.Description,

            BooleanVisualizerConfiguration.Description,
            ColorVideoVisualizerConfiguration.Description,
            DepthVideoVisualizerConfiguration.Description,
            AudioVisualizerConfiguration.Description,
            BiopacVisualizerConfiguration.Description,

            OpenFaceVisualizerConfiguration.Description,
            OpenPoseVisualizerConfiguration.Description,
            AzureKinectBodyTrackerVisualizerConfiguration.Description,
            StreamingSpeechRecognitionVisualizerConfiguration.Description,
            OpenSmileVisualizerConfiguration.Description,
            HeadGestureVisualizerConfiguration.Description,
            GazeToDisplayVisualizerConfiguration.Description,
            EmotionVisualizerConfiguration.Description,
        };

        public static ComponentDescription Description(InstanceConfiguration config) => Components.Single(d => d.ConfigurationType == config.Type);

        private static string StoreName(InstanceConfiguration instConfig, PortConfiguration portConfig) {
            var name = $"{instConfig.Name}:{portConfig.PropertyName}";
            if (portConfig.Indexer != null) {
                name += $"[{portConfig.Indexer}]";
            }
            return name;
        }

        public static PipelineEnvironment BuildPipeline(PipelineConfiguration config) {
            var pipeline = Pipeline.Create(config.Name, config.DeliveryPolicy);
            var importers = new List<InstanceEnvironment>();
            foreach (var importerConfig in config.Instances.OfType<ImporterConfiguration>()) {
                var importer = importerConfig.Instantiate(pipeline);
                var importerEnv = new InstanceEnvironment() {
                    Instance = importer,
                    Configuration = importerConfig,
                };
                importers.Add(importerEnv);
            }
            var streamReaders = new List<InstanceEnvironment>();
            foreach (var readerConfig in config.Instances.OfType<StreamReaderConfiguration>()) {
                Func<Type, dynamic> binding = (Type dataType) => readerConfig.Instantiate(pipeline, importers, dataType);
                var readerEnv = new InstanceEnvironment() {
                    Instance = binding,
                    Configuration = readerConfig,
                };
                streamReaders.Add(readerEnv);
            }
            var components = new List<InstanceEnvironment>();
            foreach (var compConfig in config.Instances.OfType<ComponentConfiguration>()) {
                var comp = compConfig.Instantiate(pipeline);
                var compEnv = new InstanceEnvironment() {
                    Instance = comp,
                    Configuration = compConfig,
                };
                components.Add(compEnv);
            }
            var operators = new List<InstanceEnvironment>();
            foreach (var opConfig in config.Instances.OfType<OperatorConfiguration>()) {
                dynamic op = opConfig.Instantiate(pipeline, components);
                var opEnv = new InstanceEnvironment() {
                    Instance = op,
                    Configuration = opConfig,
                };
                operators.Add(opEnv);
            }
            var exporters = new List<InstanceEnvironment>();
            foreach (var exporterConfig in config.Instances.OfType<ExporterConfiguration>()) {
                var exporter = exporterConfig.Instantiate(pipeline);
                var exporterEnv = new InstanceEnvironment() {
                    Instance = exporter,
                    Configuration = exporterConfig,
                };
                exporters.Add(exporterEnv);
            }
            var instances = new List<InstanceEnvironment>();
            instances.AddRange(streamReaders);
            instances.AddRange(components);
            instances.AddRange(operators);
            instances.AddRange(exporters);
            foreach (var writerConfig in config.Instances.OfType<StreamWriterConfiguration>()) {
                writerConfig.Instantiate(pipeline, instances);
            }
            foreach (var compConfig in config.Instances.OfType<ComponentConfiguration>()) {
                var comp = components.Where(c => c.Configuration.Guid == compConfig.Guid).Single();
                foreach (var inputConfig in compConfig.Inputs) {
                    if (inputConfig.Output is null || inputConfig.Remote == Guid.Empty) {
                        continue;
                    }
                    var remote = instances.Single(inst => inst.Configuration.Guid == inputConfig.Remote);
                    
                    // get emitter
                    dynamic emitter = null;
                    var remotePortDesc = Description(remote.Configuration).Outputs.Single(o => o.Name == inputConfig.Output.PropertyName);
                    switch (remotePortDesc) {
                        case VirtualPortDescription vRemote:
                            emitter = remote.Instance;
                            break;
                        case StaticPortDescription sRemote:
                            dynamic propInst = sRemote.Property.GetValue(remote.Instance);
                            if (sRemote.IsList) {
                                emitter = propInst[int.Parse(inputConfig.Output.Indexer)];
                            } else if (sRemote.IsDictionary) {
                                emitter = propInst[inputConfig.Output.Indexer];
                            } else {
                                emitter = propInst;
                            }
                            break;
                    }

                    //get receiver
                    dynamic receiver = null;
                    var localPortDesc = Description(comp.Configuration).Inputs.Single(i => i.Name == inputConfig.Input.PropertyName);
                    switch (localPortDesc) {
                        case VirtualPortDescription vLocal:
                            receiver = comp.Instance;
                            break;
                        case StaticPortDescription sLocal:
                            dynamic propInst = sLocal.Property.GetValue(comp.Instance);
                            if (sLocal.IsList) {
                                receiver = propInst[int.Parse(inputConfig.Input.Indexer)];
                            } else if (sLocal.IsDictionary) {
                                receiver = propInst[inputConfig.Input.Indexer];
                            } else {
                                receiver = propInst;
                            }
                            break;
                    }

                    //adjustments
                    switch (emitter) {
                        case Func<Type, dynamic> binding:
                            var dataType = System.Linq.Enumerable.Single(receiver.GetType().GetGenericArguments());
                            emitter = binding(dataType);
                            break;
                        default:
                            break;
                    }

                    //connect
                    if (emitter is null) {
                        throw new Exception($"Cannot find the output port {remotePortDesc.Name} of {remote.Configuration.Name}");
                    }
                    if (receiver is null) {
                        throw new Exception($"Cannot find the input port {localPortDesc.Name} of {compConfig.Name}");
                    }
                    Operators.PipeTo(emitter, receiver, inputConfig.DeliveryPolicy);
                }
            }
            
            return new PipelineEnvironment() {
                Pipeline = pipeline,
                Instances = instances,
            };
        }

        public static string Serialize(PipelineConfiguration config) {
            var jsonSettings = new JsonSerializerSettings() {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
            };
            var jsonSerializer = JsonSerializer.Create(jsonSettings);
            using (var stringWriter = new StringWriter()) {
                using (var jsonTextWriter = new JsonTextWriter(stringWriter) {
                    IndentChar = '\t',
                    Indentation = 1,
                    QuoteName = false,
                    Formatting = Formatting.Indented,
                }) {
                    jsonSerializer.Serialize(jsonTextWriter, config);
                }
                return stringWriter.ToString();
            }
        }

        public static PipelineConfiguration Deserialize(string json) {
            var setting = new JsonSerializerSettings();
            setting.Converters.Add(new InstanceConfigurationJsonConverter());
            setting.Converters.Add(new DeliveryPolicyJsonConverter());
            return JsonConvert.DeserializeObject<PipelineConfiguration>(json, setting);
        }
    }
}
