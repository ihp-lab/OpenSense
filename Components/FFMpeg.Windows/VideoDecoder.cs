using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FFMpegInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.FFMpeg;

/// <summary>
/// Decodes video packets into frames using FFmpeg.
/// Input must use a non-dropping DeliveryPolicy; dropping packets causes decoding errors.
/// </summary>
public sealed class VideoDecoder : IConsumer<Shared<Packet>>, IProducer<Shared<Frame>>, INotifyPropertyChanged, IDisposable {

    private readonly FFMpegInterop.VideoDecoder _decoder = new();
    private readonly List<Frame> _frames = new();

    #region Ports
    public Receiver<Shared<Packet>> In { get; }

    public Emitter<Shared<Frame>> Out { get; }
    #endregion

    #region Options
    private bool discardRemainingOnStop;

    /// <summary>
    /// When enabled, frames remaining in the decoder are discarded when the input ends.
    /// </summary>
    public bool DiscardRemainingOnStop {
        get => discardRemainingOnStop;
        set => SetProperty(ref discardRemainingOnStop, value);
    }

    private ILogger? logger;

    public ILogger? Logger {
        get => logger;
        set => SetProperty(ref logger, value);
    }
    #endregion

    /// <summary>
    /// Originating time of the start of the file.
    /// </summary>
    private DateTime? fileStartTime;
    private DateTime lastOriginatingTime = DateTime.MinValue;

    public VideoDecoder(Pipeline pipeline) {
        In = pipeline.CreateReceiver<Shared<Packet>>(this, Process, nameof(In));
        Out = pipeline.CreateEmitter<Shared<Frame>>(this, nameof(Out));

        In.Unsubscribed += OnUnsubscribed;
    }

    private void Process(Shared<Packet> packet, Envelope envelope) {
        // Envelope time of a packet is its decoding time
        fileStartTime ??= envelope.OriginatingTime - TimeSpan.FromTicks(packet.Resource.DecodingTimeOffset);

        _decoder.Decode(packet.Resource, _frames);
        PostFrames(finalOriginatingTime: null);
    }

    /// <summary>
    /// Flushes the decoder when the input ends.
    /// </summary>
    private void OnUnsubscribed(DateTime finalOriginatingTime) {
        if (DiscardRemainingOnStop) {
            return;
        }
        _decoder.Flush(_frames);
        PostFrames(finalOriginatingTime);
    }

    private void PostFrames(DateTime? finalOriginatingTime) {
        var late = 0;
        var unordered = 0;
        foreach (var frame in _frames) {
            using var shared = Shared.Create(frame);
            var originatingTime = (DateTime)fileStartTime! + TimeSpan.FromTicks(frame.PresentationTimeOffset);
            if (originatingTime <= lastOriginatingTime) {
                unordered++;
                continue;
            }
            if (originatingTime > finalOriginatingTime) {
                // The pipeline silently drops messages after its final originating time.
                late++;
                continue;
            }
            lastOriginatingTime = originatingTime;
            Out.Post(shared, originatingTime);
        }
        _frames.Clear();
        if (unordered > 0) {
            Logger?.LogWarning("{Count} frames whose timestamps are not later than the previous frame are discarded.", unordered);
        }
        if (late > 0) {
            Logger?.LogWarning("{Count} frames timestamped after the final originating time of the pipeline are discarded.", late);
        }
    }

    #region IDisposable
    private bool disposed;

    public void Dispose() {
        if (disposed) {
            return;
        }
        disposed = true;

        In.Unsubscribed -= OnUnsubscribed;
        foreach (var frame in _frames) {
            frame.Dispose();
        }
        _frames.Clear();
        _decoder.Dispose();
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
