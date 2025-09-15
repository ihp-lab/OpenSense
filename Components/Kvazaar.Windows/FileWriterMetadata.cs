using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using Microsoft.Psi;

namespace OpenSense.Components.Kvazaar {
    [Export(typeof(IComponentMetadata))]
    public sealed class FileWriterMetadata : IComponentMetadata {

        private static readonly FileWriterInputPortMetadata InputPortMetadata = new FileWriterInputPortMetadata();

        public string Name => "Kvazaar MP4 File Writer";

        public string Description => "[Experimental] Write 16-bit grayscale images to an MP4 file using Kvazaar HEVC encoder. This component is maded inteded for recording depth video.";

        public IReadOnlyList<IPortMetadata> Ports { get; } = [
            InputPortMetadata,
        ];

        public ComponentConfiguration CreateConfiguration() => new FileWriterConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            // FileWriter has no output ports
            Debug.Assert(false, "FileWriter has no output ports");
            return null;
        }
    }
}
