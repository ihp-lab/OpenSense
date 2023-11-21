#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Port;
using Mediapipe.Net.Framework.Protobuf;
using Mediapipe.Net.Native;

namespace OpenSense.Components.MediaPipe.NET.Packets {
    public sealed class NormalizedLandmarkListVectorPacket : Packet<List<NormalizedLandmarkList>> {

        #region cctor
        private static readonly MethodInfo GetMethod;

        private static readonly MethodInfo DeserializeMethod;

        private static readonly MethodInfo DisposeMethod;

        static NormalizedLandmarkListVectorPacket() {
            var assembly = Assembly.GetAssembly(typeof(Packet<>));
            Debug.Assert(assembly is not null);

            /* Get Mediapipe.Net.Native.UnsafeNativeMethods.mp_Packet__GetNormalizedLandmarkListVector(). */
            var type1 = assembly.GetType("Mediapipe.Net.Native.UnsafeNativeMethods", throwOnError: true);
            Debug.Assert(type1 is not null);
            var method1 = type1.GetMethod("mp_Packet__GetNormalizedLandmarkListVector", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(method1 is not null);
            GetMethod = method1;

            /* Get Mediapipe.Net.External.SerializedProtoVector.Deserialize(). */
            var type2 = assembly.GetType("Mediapipe.Net.External.SerializedProto", throwOnError: true);
            Debug.Assert(type2 is not null);
            var method2 = type2.GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(method2 is not null);
            Debug.Assert(method2.IsGenericMethodDefinition);
            DeserializeMethod = method2.MakeGenericMethod(typeof(NormalizedLandmarkList));

            /* Get SerializedProtoVector.Dispose(). */
            var method3 = type2.GetMethod("Dispose", BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(method3 is not null);
            DisposeMethod = method3;
        }
        #endregion

        #region ctor
        public NormalizedLandmarkListVectorPacket() : base(true) { }

        public NormalizedLandmarkListVectorPacket(IntPtr ptr, bool isOwner = true) : base(ptr, isOwner) { } 
        #endregion

        public NormalizedLandmarkListVectorPacket? At(Timestamp timestamp) => At<NormalizedLandmarkListVectorPacket>(timestamp);

        #region Packet<>
        public override List<NormalizedLandmarkList> Get() {
            /* Replicate: UnsafeNativeMethods.mp_Packet__GetLandmarkListVector(MpPtr, out var serializedProtoVector).Assert(); */
            var args = new object?[] { MpPtr, null, };
            var nativeRet = GetMethod.Invoke(null, args);
            Debug.Assert(nativeRet is MpReturnCode);
            var code = (MpReturnCode)nativeRet;
            code.Assert();
            var serializedProtoVector = args[1];//SerializedProtoVector is internal
            Debug.Assert(serializedProtoVector is not null);

            GC.KeepAlive(this);

            /* Replicate: var landmarkList = serializedProtoVector.Deserialize(NormalizedLandmarkList.Parser); */
            var protoRet = DeserializeMethod.Invoke(serializedProtoVector, new[] { NormalizedLandmarkList.Parser, });
            Debug.Assert(protoRet is List<NormalizedLandmarkList>);
            var landmarkList = (List<NormalizedLandmarkList>)protoRet;

            /* Replicate: serializedProtoVector.Dispose(); */
            DisposeMethod.Invoke(serializedProtoVector, null);

            return landmarkList;
        }

        public override StatusOr<List<NormalizedLandmarkList>> Consume() {
            throw new NotSupportedException();
        }

        public override Status ValidateAsType() {
            throw new NotImplementedException();
        }
        #endregion
    }
}
