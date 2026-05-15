using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Speech;

namespace OpenSense.Components.Whisper.NET {

    /// <summary>
    /// Streaming speech recognition result that also exposes the source Whisper segments.
    /// For a SingleSegment-merge result, <see cref="Segments"/> still holds the originals.
    /// </summary>
    [DataContract(Namespace = "https://github.com/ihp-lab/OpenSense/Components/Whisper.NET")]
    public sealed class WhisperStreamingSpeechRecognitionResult : StreamingSpeechRecognitionResult {

        //Concrete array (not IReadOnlyList<T>) because \psi has no serializer template for
        //the interface — only IEnumerable<T>, IDictionary<,>, and arrays are wired up.
        [DataMember(Name = nameof(Segments))]
        private readonly WhisperSegmentInfo[] _segments;

        public IReadOnlyList<WhisperSegmentInfo> Segments => _segments;

        public WhisperStreamingSpeechRecognitionResult(
            bool isFinal,
            string text,
            double? confidence,
            IEnumerable<SpeechRecognitionAlternate> alternates,
            AudioBuffer? audio,
            TimeSpan? duration,
            WhisperSegmentInfo[] segments
        ) : base(
            isFinal, text, confidence, alternates, audio, duration
        ) {
            _segments = segments;
        }
    }
}
