using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.FFMpeg;

[Serializable]
public sealed class DemuxerConfiguration : ConventionalComponentConfiguration {

    #region Options
    #region File Settings
    private string filename = string.Empty;

    public string Filename {
        get => filename;
        set => SetProperty(ref filename, value);
    }
    #endregion

    #region Timestamp Settings
    private StartTimeMode startTimeMode = StartTimeMode.PipelineStartTime;

    public StartTimeMode StartTimeMode {
        get => startTimeMode;
        set => SetProperty(ref startTimeMode, value);
    }

    private DateTime manualStartTime = DateTime.MinValue;

    public DateTime ManualStartTime {
        get => manualStartTime;
        set => SetProperty(ref manualStartTime, value);
    }
    #endregion
    #endregion

    public override IComponentMetadata GetMetadata() => new DemuxerMetadata();

    protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) {
        var instance = new Demuxer(pipeline, Filename) {
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
        try {
            instance.ManualStartTime = ManualStartTime;
            instance.StartTimeMode = StartTimeMode;
        } catch {
            instance.Dispose();
            throw;
        }
        return instance;
    }
}
