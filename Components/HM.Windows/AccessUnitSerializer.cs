using System;
using System.Buffers;
using HMInterop;
using Microsoft.Psi.Common;
using Microsoft.Psi.Serialization;

namespace OpenSense.Components.HM {
    /// <summary>
    /// Psi serializer for <see cref="HMInterop.AccessUnit"/>.
    /// Binary format: [PTS: int64][DTS: int64][AnnexB length: int32][AnnexB data: N bytes].
    /// Deserialization rebuilds the NalIndex header by scanning start codes via CreateFromAnnexB.
    /// </summary>
    public sealed class AccessUnitSerializer : ISerializer<AccessUnit> {

        private const int SchemaVersion = 1;
        private static readonly string TargetTypeName = typeof(AccessUnit).AssemblyQualifiedName!;
        private static readonly string SerializerTypeName = typeof(AccessUnitSerializer).AssemblyQualifiedName!;
        private static readonly string LongTypeName = typeof(long).AssemblyQualifiedName!;
        private static readonly string ByteArrayTypeName = typeof(byte[]).AssemblyQualifiedName!;

        #region ISerializer<AccessUnit>

        public bool? IsClearRequired => true;

        public TypeSchema Initialize(KnownSerializers serializers, TypeSchema targetSchema) {
            var name = TypeSchema.GetContractName(typeof(AccessUnit), serializers.RuntimeInfo.SerializationSystemVersion);
            var members = new[] {
                new TypeMemberSchema(name: nameof(AccessUnit.PresentationTimeOffset), type: LongTypeName, isRequired: true),
                new TypeMemberSchema(name: nameof(AccessUnit.DecodingTimeOffset), type: LongTypeName, isRequired: true),
                new TypeMemberSchema(name: nameof(AccessUnit.AnnexB), type: ByteArrayTypeName, isRequired: true),
            };
            var schema = new TypeSchema(
                TargetTypeName,
                TypeFlags.IsClass,
                members,
                name,
                TypeSchema.GetId(name),
                SchemaVersion,
                SerializerTypeName,
                serializers.RuntimeInfo.SerializationSystemVersion
            );
            return targetSchema ?? schema;
        }

        public void Serialize(BufferWriter writer, AccessUnit instance, SerializationContext context) {
            writer.Write(instance.PresentationTimeOffset);
            writer.Write(instance.DecodingTimeOffset);
            var annexB = instance.AnnexB;
            writer.Write(annexB.Length);
            if (annexB.Length <= 0) {
                return;
            }
            using var handle = annexB.Pin();
            unsafe {
                writer.Write(handle.Pointer, annexB.Length);
            }
        }

        public void PrepareDeserializationTarget(BufferReader reader, ref AccessUnit target, SerializationContext context) {
            target?.Dispose();
            target = null!;
        }

        public void Deserialize(BufferReader reader, ref AccessUnit target, SerializationContext context) {
            var pts = reader.ReadInt64();
            var dts = reader.ReadInt64();
            var length = reader.ReadInt32();
            if (length == 0) {
                target = AccessUnit.CreateFromAnnexB(ReadOnlyMemory<byte>.Empty, pts, dts);
                return;
            }
            var data = ArrayPool<byte>.Shared.Rent(length);
            try {
                reader.Read(data, length);
                target = AccessUnit.CreateFromAnnexB(new ReadOnlyMemory<byte>(data, 0, length), pts, dts);
            } finally {
                ArrayPool<byte>.Shared.Return(data);
            }
        }

        public void PrepareCloningTarget(AccessUnit instance, ref AccessUnit target, SerializationContext context) {
            target?.Dispose();
            target = null!;
        }

        public void Clone(AccessUnit instance, ref AccessUnit target, SerializationContext context) {
            target = AccessUnit.CreateFromAnnexB(
                instance.AnnexB,
                instance.PresentationTimeOffset,
                instance.DecodingTimeOffset
            );
        }

        public void Clear(ref AccessUnit target, SerializationContext context) {
            target?.Dispose();
            target = null!;
        }

        #endregion
    }
}
