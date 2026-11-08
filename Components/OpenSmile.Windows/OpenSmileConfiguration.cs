using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using OpenSense.Components.Audio;

namespace OpenSense.Components.OpenSmile;

[Serializable]
public sealed class OpenSmileConfiguration : ConventionalComponentConfiguration {

    #region Options
    private string configurationFilename = string.Empty;

    public string ConfigurationFilename {
        get => configurationFilename;
        set => SetProperty(ref configurationFilename, value);
    }

    private ObservableCollection<OpenSmileOption> options = new();

    public ObservableCollection<OpenSmileOption> Options {
        get => options;
        set => SetProperty(ref options, value);
    }

    private int openSmileLogLevel = 2; // Same as SMILEapi

    /// <summary>
    /// openSMILE verbosity for messages and warnings. Errors are always reported.
    /// </summary>
    public int OpenSmileLogLevel {
        get => openSmileLogLevel;
        set => SetProperty(ref openSmileLogLevel, value);
    }

    private TimestampMode inputTimestampMode = TimestampMode.AtEnd; // \psi convention

    public TimestampMode InputTimestampMode {
        get => inputTimestampMode;
        set => SetProperty(ref inputTimestampMode, value);
    }

    private TimestampMode outputTimestampMode = TimestampMode.AtEnd; // \psi convention

    public TimestampMode OutputTimestampMode {
        get => outputTimestampMode;
        set => SetProperty(ref outputTimestampMode, value);
    }

    private bool discardRemainingOnStop;

    public bool DiscardRemainingOnStop {
        get => discardRemainingOnStop;
        set => SetProperty(ref discardRemainingOnStop, value);
    }
    #endregion

    public override IComponentMetadata GetMetadata() => new OpenSmileMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) {
        var instance = new OpenSmile(
            pipeline,
            ConfigurationFilename,
            Options.Select(o => new KeyValuePair<string, string>(o.Name, o.Value)),
            OpenSmileLogLevel,
            (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name)
        ) {
            InputTimestampMode = InputTimestampMode,
            OutputTimestampMode = OutputTimestampMode,
            DiscardRemainingOnStop = DiscardRemainingOnStop,
        };
        try {
            ValidateInputNames(Inputs, instance);
        } catch {
            instance.Dispose();
            throw;
        }
        return instance;
    }

    /// <summary>
    /// Input names are only known once openSMILE has parsed the configuration, so the editor cannot check them.
    /// Without this check, a wrong name fails later with a bare KeyNotFoundException while connecting.
    /// </summary>
    private static void ValidateInputNames(IEnumerable<InputConfiguration> inputs, OpenSmile instance) {
        foreach (var input in inputs) {
            if (!Equals(input.LocalPort?.Identifier, nameof(OpenSmile.In))) {
                continue;
            }
            var name = input.LocalPort.Index as string;
            if (name is null || !instance.In.ContainsKey(name)) {
                var available = instance.In.Count > 0 ? string.Join(", ", instance.In.Keys) : "none";
                throw new InvalidOperationException($"The openSMILE configuration has no cExternalAudioSource named \"{name}\". Available sources: {available}.");
            }
        }
    }
}
