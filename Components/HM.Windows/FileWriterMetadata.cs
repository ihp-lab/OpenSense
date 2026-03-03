using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using HMInterop;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileWriterMetadata : IComponentMetadata {

        private static readonly ConstrainedGenericPortMetadata InputPortMetadata = new(
            typeof(FileWriter<>).GetProperty(nameof(FileWriter<ImageBase>.In))!,
            typeof(Shared<>),
            typeof(ImageBase),
            PortDirection.Input,
            "The input \\psi image stream. Only supports Gray_16bpp pixel format, because \\psi's PixelFormat has no multi-channel 16-bit format. Use either In or PictureIn, using both simultaneously is not supported."
        );
        private static readonly StaticPortMetadata PictureInputPortMetadata = new(
            typeof(FileWriter<ImageBase>).GetProperty(nameof(FileWriter<ImageBase>.PictureIn))!,
            "The input HM PicYuv stream. Supports any ChromaFormat and bit depth. Use either In or PictureIn, using both simultaneously is not supported."
        );

        public string Name => "HM MP4 File Writer";

        public string Description => "[Experimental] Write video to an MP4 file using HM (HEVC Model) encoder. Supports grayscale and YCbCr via the PictureIn port.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = [
            InputPortMetadata,
            PictureInputPortMetadata,
        ];

        public ComponentConfiguration CreateConfiguration() => new FileWriterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            // FileWriter has no output ports
            Debug.Assert(false, "FileWriter has no output ports");
            return null;
        }
    }
}
