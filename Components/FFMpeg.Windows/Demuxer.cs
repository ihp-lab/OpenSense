using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
    private readonly Dictionary<CodecParameters, Emitter<Shared<Packet>>> _emitters = new();

    #region Ports
    public ImmutableList<Emitter<Shared<Packet>>> VideoOut { get; }

    public ImmutableList<Emitter<Shared<Packet>>> AudioOut { get; }
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
    private long endOffset;
    private DateTime filenameStartTime;

    /// <summary>
    /// Originating time of the start of the file.
    /// </summary>
    private DateTime fileStartTime;

    /// <summary>
    /// Reading is paced relative to this time.
    /// </summary>
    private DateTime pacingStartTime;

    public Demuxer(Pipeline pipeline, string filename) : base(pipeline) {
        if (!File.Exists(filename)) {
            throw new FileNotFoundException($"File {filename} does not exist.");
        }
        Filename = filename;
        _reader = new FFMpegInterop.Demuxer(filename);

        VideoOut = CreateEmitters(pipeline, _reader.VideoStreams, nameof(VideoOut));
        AudioOut = CreateEmitters(pipeline, _reader.AudioStreams, nameof(AudioOut));

        pipeline.PipelineRun += OnPipelineRun;
    }

    private ImmutableList<Emitter<Shared<Packet>>> CreateEmitters(Pipeline pipeline, IReadOnlyList<CodecParameters> streams, string name) {
        var emitters = streams
            .Select((_, i) => pipeline.CreateEmitter<Shared<Packet>>(this, $"{name}[{i}]"))
            .ToImmutableList();
        for (var i = 0; i < streams.Count; i++) {
            _emitters.Add(streams[i], emitters[i]);
        }
        return emitters;
    }

    #region Pipeline Event Handlers
    private void OnPipelineRun(object? sender, PipelineRunEventArgs args) {
        fileStartTime = StartTimeMode switch {
            StartTimeMode.ParseFromFilename => filenameStartTime,
            StartTimeMode.Manual => ManualStartTime,
            _ => args.StartOriginatingTime,
        };
        // A file from the past is played now; a file from the future must wait, messages ahead of the clock block stopping
        pacingStartTime = fileStartTime > args.StartOriginatingTime ? fileStartTime : args.StartOriginatingTime;

        _reader.SelectStreams(_emitters.Where(e => e.Value.HasSubscribers).Select(e => e.Key));
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
        var due = pacingStartTime + TimeSpan.FromTicks(next.DecodingTimeOffset);
        if (due > currentTime) {
            // Not due yet, the file starts in the future
            return due;
        }

        var emitter = _emitters[next.Parameters];
        endOffset = Math.Max(endOffset, Math.Max(next.PresentationTimeOffset, next.DecodingTimeOffset) + next.Duration);
        using (var shared = Shared.Create(next)) {
            emitter.Post(shared, fileStartTime + TimeSpan.FromTicks(next.DecodingTimeOffset));
        }

        next = _reader.ReadPacket();
        if (next is null) {
            // One more call at the end of the file, data ending there must not be after the final time of the pipeline
            var end = pacingStartTime + TimeSpan.FromTicks(endOffset);
            return end > currentTime ? end : DateTime.MaxValue;
        }
        due = pacingStartTime + TimeSpan.FromTicks(next.DecodingTimeOffset);
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
