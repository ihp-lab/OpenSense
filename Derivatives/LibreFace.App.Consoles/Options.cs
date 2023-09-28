using CommandLine;

namespace LibreFace.App.Consoles {
    internal sealed class Options {

        [Option('f', "file", Required = true, HelpText = "Input MP4 video file(s).")]
        public IEnumerable<string> File { get; set; } = null!;

        [Option('o', "out", Required = false, HelpText = "Output folder.")]
        public string? Output { get; set; }
    }
}
