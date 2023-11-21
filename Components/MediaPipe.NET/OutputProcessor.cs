#nullable enable

using System;
using System.Diagnostics;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Port;
using Microsoft.Psi;

namespace OpenSense.Components.MediaPipe.NET {
    internal sealed class OutputProcessor<T> where T : notnull {

        private readonly Emitter<T?> _emitter;

        public OutputProcessor(Emitter<T?> emitter) {
            _emitter = emitter;
        }

        internal void Process(Packet<T> packet) {//From MediaPipe.NET 0.9.1, This method cannot be used with ObserveOutputStream(). "System.argumentexception: Object contains references. (parameter 'value')" will be thrown by the method when trying to pin the callback. That method might have a bug. The callback is not need to be pinned, so we provide our low-level callback "NativePacketCallback" and use another overload.
            var data = packet.Get();
            var timestamp = packet.Timestamp();
            var dateTime = new DateTime(timestamp.Value, DateTimeKind.Utc);
            _emitter.Post(data, dateTime);
        }
        
        internal Status.StatusArgs NativePacketCallback<TPacket>(IntPtr graphPtr, int streamId, IntPtr packetPtr) where TPacket : Packet<T>, new() {
            var packet = Packet<T>.Create<TPacket>(packetPtr, isOwner: false);
            Debug.Assert(packet is not null);
            Process(packet);
            return Status.StatusArgs.Ok();
        }
    }
}
