using CommandLine;

namespace LibreFace.Benchmarks2 {
    public sealed class Options {

        [Option('d', "dir", Default = "C:\\D\\Projects\\Other\\LibreFace_TestData\\DISFA\\images\\SN001", HelpText = "A folder which contains unaligned test images.")]
        public string ImageDirectory { get; set; }

        [Option('i', "image", Default = int.MaxValue, HelpText = "Limit the number of images to use.")]
        public int MaxImageCount { get; set; }

        [Option('w', "warmup", Default = 2, HelpText = "Number of warm up loops.")]
        public int WarmupLoops { get; set; }

        [Option('t', "test", Default = 5, HelpText = "Number of test loops.")]
        public int TestLoops { get; set; }

        [Option('r', "thread", Default = 1, HelpText = "Microsoft \\psi thread count. 0 means no limit.")]
        public int ThreadCount { get; set; }
    }
}
