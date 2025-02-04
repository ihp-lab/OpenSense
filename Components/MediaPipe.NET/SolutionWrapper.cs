﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using Newtonsoft.Json.Linq;

namespace OpenSense.Components.MediaPipe.NET {
    internal sealed class SolutionWrapper : IDisposable {

        private readonly ILogger<SolutionWrapper>? _logger;
        private readonly CalculatorGraph _graph;
        private readonly SidePackets _sidePackets;
        private readonly Dictionary<string, GCHandle> _observeStreamHandles = new Dictionary<string, GCHandle>();

        private readonly Dictionary<string, IReceiver> _inputs = new Dictionary<string, IReceiver>();
        private readonly Dictionary<string, IEmitter> _outputs = new Dictionary<string, IEmitter>();

        public IReadOnlyDictionary<string, IReceiver> Inputs => _inputs;
        public IReadOnlyDictionary<string, IEmitter> Outputs => _outputs;

        public SolutionWrapper(
            Pipeline pipeline,
            IReadOnlyList<SidePacketConfiguration> inputSidePackets,
            IReadOnlyList<InputStreamConfiguration> inputs,
            IReadOnlyList<OutputStreamConfiguration> outputs,
            string graphPath,
            ILogger<SolutionWrapper>? logger
            ) {
            _logger = logger;

            #region Create Graph
            var graph = File.ReadAllText(graphPath);
            var config = CalculatorGraphConfig.Parser.ParseFromTextFormat(graph);
            Debug.Assert(config is not null);
            _graph = new CalculatorGraph(config);//CalculatorGraph(string) versions in the MediaPipe.NET and MediaPipeUnityPlugin mismatch, so we use other overloads to avoide errors. MediaPipe.NET calls an deleted native method, where MediaPipeUnityPlugin calls CalculatorGraphConfig.Parser.ParseFromTextFormat() in managed runtime. 
            #endregion

            #region Wire Graph Outputs
            foreach (var output in outputs) {
                IEmitter emitter;
                GCHandle handle;
                switch (output.PacketType) {
                    case PacketType.NormalizedLandmarkListVector:
                        var e = pipeline.CreateEmitter<List<NormalizedLandmarkList>?>(this, output.Identifier);
                        emitter = e;
                        var proc = new OutputProcessor<List<NormalizedLandmarkList>>(output.PacketType, e);
                        _graph.ObserveOutputStream(output.Identifier, proc.Process, out handle).AssertOk();
                        break;
                    //TODO: Add more
                    default:
                        throw new NotImplementedException();
                }
                _outputs.Add(output.Identifier, emitter);
                _observeStreamHandles.Add(output.Identifier, handle);
            }
            #endregion

            #region Prepare Graph Input Side Packets
            _sidePackets = new SidePackets();
            foreach (var inputSidePacket in inputSidePackets) {
                Packet packet;
                var jValue = inputSidePacket.Value as JValue;
                switch (inputSidePacket.PacketType) {
                    case PacketType.Bool:
                        if (jValue is null || jValue.Type != JTokenType.Boolean) {
                            throw new FormatException($"Invalid Side Packet \"{inputSidePacket.Identifier}\" JSON value.");
                        }
                        var b = (bool)jValue;
                        packet = PacketFactory.BoolPacket(b);
                        break;
                    case PacketType.Int:
                        if (jValue is null || jValue.Type != JTokenType.Integer) {
                            throw new FormatException($"Invalid Side Packet \"{inputSidePacket.Identifier}\" JSON value.");
                        }
                        var i = (int)jValue;
                        packet = PacketFactory.IntPacket(i);
                        break;
                    //TODO: Add more
                    default:
                        throw new NotImplementedException();
                }
                _sidePackets.Emplace(inputSidePacket.Identifier, packet);
            }
            #endregion

            #region Run Graph
            _graph.StartRun(_sidePackets).AssertOk();//TODO: Run graph after pipeline run. 
            #endregion

            #region Wire Graph Inputs
            foreach (var input in inputs) {
                IReceiver receiver;
                switch (input.PacketType) {
                    case PacketType.ImageFrame:
                        var imageProc = new InputProcessor<Shared<Image>>(input.Identifier, MediaPipeInteropHelpers.ConvertImage, _graph);
                        receiver = pipeline.CreateReceiver<Shared<Image>>(this, imageProc.Process, input.Identifier);
                        break;
                    //TODO: Add more
                    default:
                        throw new NotImplementedException();
                }
                _inputs.Add(input.Identifier, receiver);
            } 
            #endregion
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() {
            if (disposed) {
                return;
            }

            foreach (var stream in _inputs.Keys) {
                _graph.CloseInputStream(stream).AssertOk();
            }
            _graph.WaitUntilDone().AssertOk();
            _graph.Dispose();

            _sidePackets.Dispose();

            foreach (var handle in _observeStreamHandles.Values) {
                handle.Free();
            }

            disposed = true;
        }
        #endregion
    }
}
