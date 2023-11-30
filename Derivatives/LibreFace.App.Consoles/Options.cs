using CommandLine;

namespace LibreFace.App.Consoles {
    internal sealed class Options {

        [Value(0, MetaName = "files", Required = true, HelpText = "Input video file(s).")]
        public IEnumerable<string> File { get; set; } = null!;

        [Option('o', "out", Required = false, HelpText = "Alternative output folder.")]
        public string? Output { get; set; }

        [Option('m', "maximum", Required = false, HelpText = "Maximum number of faces.", Default = 1)]
        public int NumFaces { get; set; }
    }
}
