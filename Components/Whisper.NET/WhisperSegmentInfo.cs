using System;
using System.Runtime.Serialization;
using Whisper.net;

namespace OpenSense.Components.Whisper.NET {

    /// <summary>
    /// Immutable, \psi-serializable snapshot of <see cref="SegmentData"/>. Drops Tokens
    /// (unused downstream).
    /// </summary>
    [DataContract(Namespace = "https://github.com/ihp-lab/OpenSense/Components/Whisper.NET")]
    public sealed record class WhisperSegmentInfo(
        [property: DataMember] string Text,
        [property: DataMember] TimeSpan Start,
        [property: DataMember] TimeSpan End,
        [property: DataMember] float MinProbability,
        [property: DataMember] float MaxProbability,
        [property: DataMember] float Probability,
        [property: DataMember] float NoSpeechProbability,
        [property: DataMember] string Language
    ) {

        public static explicit operator WhisperSegmentInfo(SegmentData segment) => new(
            segment.Text,
            segment.Start,
            segment.End,
            segment.MinProbability,
            segment.MaxProbability,
            segment.Probability,
            segment.NoSpeechProbability,
            segment.Language);
    }
}
