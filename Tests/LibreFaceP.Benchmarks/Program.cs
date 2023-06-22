using BenchmarkDotNet.Running;

namespace LibreFace.Benchmarks {
    internal static class Program {
        public static void Main(string[] args) {
            var summary = BenchmarkRunner.Run<VsOpenFace>();
        }
    }
}
