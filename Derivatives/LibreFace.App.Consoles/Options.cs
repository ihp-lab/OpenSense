using CommandLine;

namespace LibreFace.App.Consoles {
    internal sealed class Options {

        [Value(0, MetaName = "paths", Required = true, HelpText = "Input video file or directory name(s).")]
        public IEnumerable<string> Paths { get; set; } = null!;

        [Option('o', "out", Required = false, HelpText = "Alternative output folder.")]
        public string? Output { get; set; }

        [Option('m', "maximum", Required = false, HelpText = "Maximum number of faces.", Default = 1)]
        public int NumFaces { get; set; }

        [Option('c', "concurrency", Required = false, HelpText = "Maximum degree of parallelism. Applicable only with multiple inputs.", Default = 1)]
        public int Concurrency { get; set; }

        [Option('d', "device", Required = false, HelpText = "CUDA device ID. (Not Recommended) Use a negative value to use CPU for inference.", Default = 0)]
        public int DeviceId { get; set; }

        [Option('s', "succinct", Required = false, HelpText = "Succinct UI mode. Good for batch processing.")]
        public bool Succinct { get; set; }
    }
}
