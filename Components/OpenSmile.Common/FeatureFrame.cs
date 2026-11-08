using System;

namespace OpenSense.Components.OpenSmile;

/// <summary>
/// A feature vector produced by openSMILE.
/// Its originating time is the start or the end of the audio it covers, as selected by the OutputTimestampMode of the component, and Duration gives the other end.
/// No absolute time is stored, so that the covered interval follows the originating time when a stream is retimed.
/// The feature names are posted once, on the FeatureNamesOut port of the same sink.
/// </summary>
/// <param name="Index">Index of the frame in the openSMILE data memory level read by the sink.</param>
/// <param name="Duration">Length of audio covered by the frame.</param>
/// <param name="Values">Feature values, in the order of the feature names.</param>
[Serializable]
public sealed record class FeatureFrame(
    long Index,
    TimeSpan Duration,
    float[] Values
);
