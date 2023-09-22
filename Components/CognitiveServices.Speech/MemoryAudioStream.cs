using System;
using Microsoft.CognitiveServices.Speech.Audio;

namespace OpenSense.Components.CognitiveServices.Speech {
    internal sealed class MemoryAudioStream : PullAudioInputStreamCallback {
        public override int Read(byte[] dataBuffer, uint size) => throw new NotImplementedException();
    }
}
