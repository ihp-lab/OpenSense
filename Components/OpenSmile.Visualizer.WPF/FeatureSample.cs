using System;

namespace OpenSense.Components.OpenSmile.Visualizer;

/// <param name="Time">Originating time of the frame. Whether it is the start or the end of the frame does not matter for plotting, since all frames of a stream follow the same convention.</param>
public readonly record struct FeatureSample(DateTime Time, float[] Values);
