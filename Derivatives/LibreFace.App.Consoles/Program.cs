using CommandLine;

namespace LibreFace.App.Consoles {
    internal static class Program {
        public static void Main(string[] args) {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run);
        }

        private static void Run(Options options) {
            var files = options.File.ToArray();
            foreach (var filename in files) {
                var dir = options.Output ?? Path.GetDirectoryName(filename) ?? throw new ArgumentNullException();
                using var progress = new Progress(filename);
                using var processor = new Processor(filename, dir, progress);
                processor.WaitTask.Wait();
            }
            
        }
    }
}
