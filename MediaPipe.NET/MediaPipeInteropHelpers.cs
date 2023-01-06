#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mediapipe.Net.Framework;
using Mediapipe.Net.Framework.Format;
using Mediapipe.Net.Framework.Packets;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using Newtonsoft.Json.Linq;

namespace OpenSense.Component.MediaPipe.NET {
    public static class MediaPipeInteropHelpers {

        public static Type MapInputType(PacketType packetType) => packetType switch {
            PacketType.Bool => typeof(bool),
            PacketType.Int => typeof(int),
            PacketType.ImageFrame => typeof(Shared<Image>),
            PacketType.NormalizedLandmarkListVector => typeof(IReadOnlyList<NormalizedLandmarkList>),
            //TODO: add more
            _ => throw new NotImplementedException(),
        };

        public static Type MapOutputType(PacketType packetType) => packetType switch {
            PacketType.Bool => typeof(bool),
            PacketType.Int => typeof(int),
            PacketType.ImageFrame => typeof(Shared<Image>),
            PacketType.NormalizedLandmarkListVector => typeof(List<NormalizedLandmarkList>),
            //TODO: add more
            _ => throw new NotImplementedException(),
        };

        public static JToken CreateDefaultValue(PacketType packetType) => packetType switch {
            PacketType.Bool => new JValue(default(bool)),
            PacketType.Int => new JValue(default(int)),
            //TODO: add more
            _ => throw new NotImplementedException(),
        };

        internal static unsafe ImageFrame CreateImageFrame(Image image) {
            var img = image.PixelFormat == PixelFormat.RGB_24bpp ? 
                image : image.Convert(PixelFormat.RGB_24bpp);//The default image format from Media Capturer is BGR_24bpp.
            var span = new ReadOnlySpan<byte>(img.UnmanagedBuffer.Data.ToPointer(), img.UnmanagedBuffer.Size);//Unsafe
            var result = new ImageFrame(Mediapipe.Net.Framework.Format.ImageFormat.Srgb, img.Width, img.Height, img.Stride, span);
            if (!ReferenceEquals(img, image)) {
                img.Dispose();
            }
            return result;
        }

        internal static Packet ConvertImage(Shared<Image> data, Envelope envelope) {
            var frame = CreateImageFrame(data.Resource);//Disposed by PacketFactory.ImageFramePacket
            Debug.Assert(envelope.OriginatingTime.Kind == DateTimeKind.Utc);
            var ticks = envelope.OriginatingTime.Ticks;
            var timestamp = new Timestamp(ticks);
            var result = PacketFactory.ImageFramePacket(frame, timestamp);
            return result;
        }
    }
}
