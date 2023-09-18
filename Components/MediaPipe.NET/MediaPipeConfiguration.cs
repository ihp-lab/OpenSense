#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Mediapipe.Net.Framework.Packets;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Newtonsoft.Json;

namespace OpenSense.Components.MediaPipe.NET {
    [Serializable]
    public class MediaPipeConfiguration : ComponentConfiguration {

        private string graph = "mediapipe/modules/face_landmark/face_landmark_front_cpu.pbtxt";

        public string Graph {
            get => graph;
            set => SetProperty(ref graph, value);
        }

        private ObservableCollection<SidePacketConfiguration> inputSidePackets = new() {
            new SidePacketConfiguration() {
                Identifier = "use_prev_landmarks",
                PacketType = PacketType.Bool,
                Value = true,
            },
            new SidePacketConfiguration() {
                Identifier = "num_faces",
                PacketType = PacketType.Int,
                Value = 1,
            },
            new SidePacketConfiguration() {
                Identifier = "with_attention",
                PacketType = PacketType.Bool,
                Value = false,
            },
        };

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]//Otherwise, elements will be ADDED to the existing collection, with sample data remained.
        public ObservableCollection<SidePacketConfiguration> InputSidePackets {
            get => inputSidePackets;
            set => SetProperty(ref inputSidePackets, value);
        }

        private ObservableCollection<InputStreamConfiguration> inputStreams = new() {
            new InputStreamConfiguration {
                Identifier = "image",
                PacketType = PacketType.ImageFrame,
            },
        };

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]//Otherwise, elements will be ADDED to the existing collection, with sample data remained.
        public ObservableCollection<InputStreamConfiguration> InputStreams {
            get => inputStreams;
            set => SetProperty(ref inputStreams, value);
        }

        private ObservableCollection<OutputStreamConfiguration> outputStreams = new() {
            new OutputStreamConfiguration {
                Identifier = "multi_face_landmarks",
                PacketType = PacketType.NormalizedLandmarkListVector,
            },
        };

        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]//Otherwise, elements will be ADDED to the existing collection, with sample data remained.
        public ObservableCollection<OutputStreamConfiguration> OutputStreams {
            get => outputStreams;
            set => SetProperty(ref outputStreams, value);
        }

        //TODO: Define input side packets. How to achieve seamless searialization/deserialization base on packet type?

        public override IComponentMetadata GetMetadata() => new MediaPipeMetadata(
            getPortsCallback: GetComponentPorts
        );

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            var logger = serviceProvider?.GetService(typeof(ILogger<SolutionWrapper>)) as ILogger<SolutionWrapper>;
            var result = new SolutionWrapper(pipeline, inputSidePackets, inputStreams, outputStreams, Graph, logger);

            foreach (var inputConfig in Inputs) {
                var inputMetadata = (StaticPortMetadata)this.FindPortMetadata(inputConfig.LocalPort);

                Debug.Assert(inputMetadata.Direction == PortDirection.Input);
                var key = (string)inputConfig.LocalPort.Identifier;
                dynamic consumer = result.Inputs[key];

                var remoteEnvironment = instantiatedComponents.Single(e => inputConfig.RemoteId == e.Configuration.Id);
                var remoteOutputMetadata = remoteEnvironment.FindPortMetadata(inputConfig.RemotePort);
                Debug.Assert(remoteOutputMetadata.Direction == PortDirection.Output);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))!
                    .MakeGenericMethod(inputMetadata.DataType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { remoteEnvironment, inputConfig.RemotePort })!;

                Operators.PipeTo(producer, consumer, inputConfig.DeliveryPolicy);
            }

            return result;
        }

        private IReadOnlyList<IPortMetadata> GetComponentPorts() {
            var result = new List<IPortMetadata>();
            foreach (var input in InputStreams) {
                var type = MediaPipeInteropHelpers.MapInputType(input.PacketType);
                var port = new StaticPortMetadata(input.Identifier, PortDirection.Input, PortAggregation.Object, type, "Input stream.");
                result.Add(port);
            }
            foreach (var output in OutputStreams) {
                var type = MediaPipeInteropHelpers.MapOutputType(output.PacketType);
                var port = new StaticPortMetadata(output.Identifier, PortDirection.Output, PortAggregation.Object, type, "Output stream.");
                result.Add(port);
            }
            return result;
        }
    }
}
