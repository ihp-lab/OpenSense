#nullable enable

using System;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Packets;
using Microsoft.Psi;

namespace OpenSense.Component.MediaPipe.NET {
    internal sealed class InputProcessor<T> where T : notnull {
        private readonly string _name;
        private readonly Func<T, Envelope, Packet> _conv;
        private readonly CalculatorGraph _graph;

        public InputProcessor(string name, Func<T, Envelope, Packet> conv, CalculatorGraph graph) {
            _name = name;
            _conv = conv;
            _graph = graph;
        }

        public void Process(T data, Envelope envelope) {
            var packet = _conv(data, envelope);
            _graph.AddPacketToInputStream(_name, packet);
            _graph.WaitUntilIdle().AssertOk();
        }
    }
}
