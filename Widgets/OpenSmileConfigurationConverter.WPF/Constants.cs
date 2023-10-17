namespace OpenSense.Widgets.OpenSmileConfigurationConverter {
    internal static class Constants {

        public static readonly string NEW_LINE = "\n";//Environment.NewLine;

        public const string RAW_WAVE_SOURCE_NAME = "cRawWaveSource";

        public const string RAW_WAVE_SOURCE_PROPERTY_DEFAULT_TEXT = @";monoMixdown = 1
;sampleRate = 16000
;sampleSize = 2
;channels = 2
;outFieldName = pcm";


        public const string RAW_DATA_SINK_NAME = "cRawDataSink";

        public const string RAW_DATA_SINK_PROPERTY_DEFAULT_TEXT = @"fieldInfo = 1";

        public static readonly string[] DATA_SOURCE_DERIVATES = {
            //"cArffSource",
            //"cCsvSource",
            //"cExampleSource",
            //"cHtkSource",
            //"cOpenCVSource",
            "cPortaudioSource",
            "cWaveSource",
        };

        public static readonly string[] RAW_DATA_SOURCE_OPTIONS = {
            //from cDataSource
            "writer",
            "buffersize",
            "buffersize_sec",
            "blocksize",
            "blocksizeW",
            "blocksize_sec",// default changed in cRawWaveSource
            "blocksizeW_sec",
            "period",// removed in cRawWaveSource
            "basePeriod",
            //from cRawWaveSource
            "monoMixdown",
            "sampleRate",
            "sampleSize",
            "channels",
            "outFieldName",
        };

        public static readonly string[] DATA_SINK_DERIVATES = {
            "cArffSink",
            "cCsvSink",
            "cDatadumpSink",
            "cHtkSink",
            "cJuliusSink",
            "cLibsvmLiveSink",
            "cLibsvmSink",
            "cNullSink",
            "cRnnSink",
            "cSvmSink",
            "cSvmSink",
            "cWaveSink",
        };

        public static readonly string[] RAW_DATA_SINK_OPTIONS = {
            //from cDataSink
            "reader",
            "blocksize",
            "blocksizeR",
            "blocksize_sec",
            "blocksizeR_sec",
            "errorOnNoOutput",
            //from cRawDataSink
            "fieldInfo",
        };

    }
}
