namespace OpenSense.Components.FFMpeg;

/// <summary>
/// How the start time of a file is determined.
/// </summary>
public enum StartTimeMode {
    PipelineStartTime,
    ParseFromFilename,
    Manual,
}
