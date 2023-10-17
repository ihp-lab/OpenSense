#nullable enable

using System.Diagnostics;
using AvalonDock.Layout;

namespace OpenSense.WPF.Views.Runners {
    /// <summary>
    /// For easier debugging.
    /// </summary>
    internal sealed class TaggedLayoutPanel : LayoutPanel {

        public string Tag { get; }

        public TaggedLayoutPanel(string tag) {
            Tag = tag;
        }

        public override void ConsoleDump(int tab) {
            Trace.Write(new string(' ', tab * 4));
            Trace.WriteLine($"Panel({Tag})");
            foreach (LayoutElement child in Children) {
                child.ConsoleDump(tab + 1);
            }
        }
    }
}
