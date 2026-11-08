using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Psi;

namespace OpenSense.Components.OpenSmile.Visualizer;

/// <summary>
/// Keeps the recent feature frames of one openSMILE sink for plotting.
/// Receivers only buffer data; the UI pulls snapshots, so drawing never blocks the pipeline.
/// </summary>
public sealed class OpenSmileVisualizer : IConsumer<FeatureFrame>, INotifyPropertyChanged {

    private readonly object _lock = new();
    private readonly Queue<FeatureSample> _samples = new();

    #region Ports
    public Receiver<FeatureFrame> In { get; }

    public Receiver<string[]> FeatureNamesIn { get; }
    #endregion

    #region Options
    private TimeSpan window = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Length of history kept for plotting.
    /// </summary>
    public TimeSpan Window {
        get => window;
        set => SetProperty(ref window, value);
    }
    #endregion

    private IReadOnlyList<string> featureNames = Array.Empty<string>();

    /// <summary>
    /// Names from FeatureNamesIn, or "#index" placeholders when the names are not connected.
    /// </summary>
    public IReadOnlyList<string> FeatureNames {
        get => featureNames;
        private set => SetProperty(ref featureNames, value);
    }

    private int version;

    /// <summary>
    /// Incremented whenever the buffered data changes, so that the UI can skip redundant redraws.
    /// </summary>
    public int Version {
        get {
            lock (_lock) {
                return version;
            }
        }
    }

    private bool hasReceivedNames;

    public OpenSmileVisualizer(Pipeline pipeline) {
        In = pipeline.CreateReceiver<FeatureFrame>(this, ProcessFrame, nameof(In));
        FeatureNamesIn = pipeline.CreateReceiver<string[]>(this, ProcessNames, nameof(FeatureNamesIn));
    }

    /// <summary>
    /// Copies the buffered samples. Safe to call from any thread.
    /// </summary>
    public FeatureSample[] GetSnapshot() {
        lock (_lock) {
            return _samples.ToArray();
        }
    }

    #region Receivers
    private void ProcessFrame(FeatureFrame frame, Envelope envelope) {
        // Messages are only valid during the callback, so the values are copied.
        var sample = new FeatureSample(envelope.OriginatingTime, frame.Values.ToArray());
        lock (_lock) {
            _samples.Enqueue(sample);
            while (_samples.Count > 0 && _samples.Peek().Time < sample.Time - Window) {
                _samples.Dequeue();
            }
            version++;
        }
        if (!hasReceivedNames && FeatureNames.Count != sample.Values.Length) {
            FeatureNames = Enumerable.Range(0, sample.Values.Length).Select(i => $"#{i}").ToArray();
        }
    }

    private void ProcessNames(string[] names, Envelope envelope) {
        hasReceivedNames = true;
        FeatureNames = names.ToArray();
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
