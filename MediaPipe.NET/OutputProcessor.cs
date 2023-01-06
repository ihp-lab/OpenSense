#nullable enable

using System;
using System.Reflection;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Port;
using Microsoft.Psi;

namespace OpenSense.Components.MediaPipe.NET {
    internal sealed class OutputProcessor<T> where T : notnull {

        private delegate void TypeSetterMethod(Packet obj, PacketType value);

        private readonly PacketType _desiredType;
        private readonly Emitter<T?> _emitter;
        private readonly TypeSetterMethod _setTypeMethod;

        public OutputProcessor(PacketType desiredType, Emitter<T?> emitter) {
            _desiredType = desiredType;
            _emitter = emitter;
            var method = typeof(Packet).GetProperty(nameof(Packet.PacketType), BindingFlags.Instance | BindingFlags.Public)
                ?.GetSetMethod(nonPublic: true)
                ?? throw new InvalidOperationException($"Cound not find the setter method of {nameof(Packet)}.{nameof(Packet.PacketType)} property.");
            _setTypeMethod = (TypeSetterMethod)Delegate.CreateDelegate(typeof(TypeSetterMethod), method);
        }

        public Status Process(Packet packet) {
            /**Note:
             * Packet.Get() relies on packet type property.
             * However, its setter is not public, and its value is not correct when returned from Graph.
             * We have to modify it.
             */
            _setTypeMethod(packet, _desiredType);
            var data = (T?)packet.Get();
            var timestamp = packet.Timestamp();
            var dateTime = new DateTime(timestamp.Value, DateTimeKind.Utc);
            _emitter.Post(data, dateTime);
            return Status.Ok();
        }
    }
}
