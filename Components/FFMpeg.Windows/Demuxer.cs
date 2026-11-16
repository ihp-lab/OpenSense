using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace OpenSense.Components.FFMpeg;

/// <summary>
/// Reads video and audio packets from a media file using FFmpeg.
/// </summary>
public sealed class Demuxer : Generator, INotifyPropertyChanged, IDisposable {

    private const string TimestampFormat = "yyyyMMddHHmmssfffffff";

    private static readonly Regex TimestampPattern = new Regex(@"_(\d{21})$", RegexOptions.Compiled);

    private readonly FFMpegInterop.Demuxer _reader;

    #region Ports
    public Emitter<Shared<Packet>> VideoOut { get; }

    public Emitter<Shared<Packet>> AudioOut { get; }
    #endregion

    #region Options
    public string Filename { get; }

    #region Timestamp Settings
    private StartTimeMode startTimeMode = StartTimeMode.PipelineStartTime;

    public StartTimeMode StartTimeMode {
        get => startTimeMode;
        set {
            // Parsed here so that a bad filename is reported at instantiation, not when the pipeline starts
            if (value == StartTimeMode.ParseFromFilename) {
                filenameStartTime = ParseTimestampFromFilename();
            }
            SetProperty(ref startTimeMode, value);
        }
    }

    private DateTime manualStartTime = DateTime.MinValue;

    public DateTime ManualStartTime {
        get => manualStartTime;
        set => SetProperty(ref manualStartTime, value);
    }
    #endregion

    private ILogger? logger;

    public ILogger? Logger {
        get => logger;
        set => SetProperty(ref logger, value);
    }
    #endregion

    private Packet? next;
    private bool started;
    private DateTime filenameStartTime;

    /// <summary>
    /// Originating time of the start of the file.
    /// </summary>
    private DateTime fileStartTime;

    /// <summary>
    /// Start time of the pipeline, reading is paced by it.
    /// </summary>
    private DateTime pipelineStartTime;

    public Demuxer(Pipeline pipeline, string filename) : base(pipeline) {
        if (!File.Exists(filename)) {
            throw new FileNotFoundException($"File {filename} does not exist.");
        }
        Filename = filename;
        _reader = new FFMpegInterop.Demuxer(filename);

        VideoOut = pipeline.CreateEmitter<Shared<Packet>>(this, nameof(VideoOut));
        AudioOut = pipeline.CreateEmitter<Shared<Packet>>(this, nameof(AudioOut));

        pipeline.PipelineRun += OnPipelineRun;
    }

    #region Pipeline Event Handlers
    private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        pipelineStartTime = args.StartOriginatingTime;
        fileStartTime = StartTimeMode switch {
            StartTimeMode.ParseFromFilename => filenameStartTime,
            StartTimeMode.Manual => ManualStartTime,
            _ => args.StartOriginatingTime,
        };

        _reader.SelectStreams(VideoOut.HasSubscribers, AudioOut.HasSubscribers);
        if (VideoOut.HasSubscribers && _reader.VideoParameters is null) {
            Logger?.LogWarning("File {Filename} has no video.", Filename);
        }
        if (AudioOut.HasSubscribers && _reader.AudioParameters is null) {
            Logger?.LogWarning("File {Filename} has no audio.", Filename);
        }
    }
    #endregion

    #region Generator
    protected override DateTime GenerateNext(DateTime currentTime) {
        if (!started) {
            started = true;
            next = _reader.ReadPacket();
        }
        if (next is null) {
            return DateTime.MaxValue;
        }

        var emitter = next.Parameters.MediaType == MediaType.Video ? VideoOut : AudioOut;
        using (var shared = Shared.Create(next)) {
            emitter.Post(shared, fileStartTime + TimeSpan.FromTicks(next.DecodingTimeOffset));
        }

        next = _reader.ReadPacket();
        if (next is null) {
            return DateTime.MaxValue;
        }
        var due = pipelineStartTime + TimeSpan.FromTicks(next.DecodingTimeOffset);
        // Returned times must strictly increase, packets of different streams are not strictly ordered
        return due > currentTime ? due : currentTime.AddTicks(1);
    }
    #endregion

    #region Helpers
    private DateTime ParseTimestampFromFilename() {
        var baseName = Path.GetFileNameWithoutExtension(Filename);
        var match = TimestampPattern.Match(baseName);
        if (!match.Success) {
            throw new InvalidOperationException($"StartTimeMode is ParseFromFilename but filename '{Path.GetFileName(Filename)}' does not contain a timestamp suffix in the expected format '_{TimestampFormat}'.");
        }
        return DateTime.ParseExact(match.Groups[1].Value, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }
    #endregion

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        next?.Dispose();
        next = null;
        _reader.Dispose();
    }
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
        if (!EqualityComparer<T>.Default.Equals(field, value)) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    #endregion
}
