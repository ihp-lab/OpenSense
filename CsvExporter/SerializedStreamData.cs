using System.Collections.Generic;
using Microsoft.Psi;

namespace OpenSense.Components.CsvExporter {
    internal class SerializedStreamData {

        public Envelope Envelope { get; set; }

        public IReadOnlyList<string> ColumnStringValues { get; set; }
    }
}
