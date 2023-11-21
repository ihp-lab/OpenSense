#nullable enable

using System;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Packets;
using Microsoft.Psi;

namespace OpenSense.Components.MediaPipe.NET {

    internal sealed class InputProcessor<TPsi, TMediaPipe> where TPsi : notnull {
        private readonly string _name;
        private readonly Func<TPsi, Envelope, Packet<TMediaPipe>> _conv;
        private readonly CalculatorGraph _graph;

        public InputProcessor(string name, Func<TPsi, Envelope, Packet<TMediaPipe>> conv, CalculatorGraph graph) {
            _name = name;
            _conv = conv;
            _graph = graph;
        }

        public void Process(TPsi data, Envelope envelope) {
            var packet = _conv(data, envelope);
            _graph.AddPacketToInputStream(_name, packet);
            _graph.WaitUntilIdle().AssertOk();
        }
    }
}
