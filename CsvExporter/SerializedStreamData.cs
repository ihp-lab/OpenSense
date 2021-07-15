using System.Collections.Generic;
using Microsoft.Psi;

namespace OpenSense.Component.CsvExporter {
    internal class SerializedStreamData {

        public Envelope Envelope { get; set; }

        public IReadOnlyList<string> ColumnStringValues { get; set; }
    }
}
