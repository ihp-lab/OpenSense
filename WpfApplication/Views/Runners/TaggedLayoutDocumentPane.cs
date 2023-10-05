#nullable enable

using System.Diagnostics;
using AvalonDock.Layout;

namespace OpenSense.WPF.Views.Runners {
    /// <summary>
    /// For easier debugging.
    /// </summary>
    internal sealed class TaggedLayoutDocumentPane : LayoutDocumentPane {

        public string Tag { get; }

        public TaggedLayoutDocumentPane(LayoutContent firstChild, string tag) : base(firstChild) {
            Tag = tag;
        }

        public override void ConsoleDump(int tab) {
            Trace.Write(new string(' ', tab * 4));
            Trace.WriteLine($"DocumentPane({Tag})");
            foreach (LayoutContent child in Children) {
                child.ConsoleDump(tab + 1);
            }
        }
    }
}
