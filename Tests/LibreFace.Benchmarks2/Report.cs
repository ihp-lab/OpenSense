using System;

namespace LibreFace.Benchmarks2 {
    public sealed class Report {

        public string Name { get; set; }

        public int Images { get; set; }

        public TimeSpan Avg { get; set; }

        public TimeSpan Std { get; set; }

        public override string ToString() => $"{Name}: avg = {Avg.TotalSeconds:F3}, std = {Std.TotalSeconds:F3}, fps = {Images / Avg.TotalSeconds}";
    }
}
