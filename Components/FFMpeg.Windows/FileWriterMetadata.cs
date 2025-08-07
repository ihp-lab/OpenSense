using System;
using System.Composition;

namespace OpenSense.Components.FFMpeg {
    [Export(typeof(IComponentMetadata))]
    public class FileWriterMetadata : ConventionalComponentMetadata {

        public override string Name => "FFMpeg MP4 File Writer";

        public override string Description => "[Experimental] Write images to a mp4 file using FFMpeg. The used FFMpeg is a regular and LGPL version and is dynamically linked.";

        protected override Type ComponentType => typeof(FileWriter);

        protected override string? GetPortDescription(string portName) {
            switch (portName) {
                case nameof(FileWriter.FrameIn):
                    return $"[Required] The input FFMpeg frames. Use either {nameof(FileWriter.FrameIn)} or {nameof(FileWriter.ImageIn)}, using both is not recommended.";
                case nameof(FileWriter.ImageIn):
                    return $"[Required] The input \\psi images. Use either {nameof(FileWriter.FrameIn)} or {nameof(FileWriter.ImageIn)}, using both is not recommended.";
                default:
                    return null;
            }
        }

        public override ComponentConfiguration CreateConfiguration() => new FileWriterConfiguration();
    }
}

