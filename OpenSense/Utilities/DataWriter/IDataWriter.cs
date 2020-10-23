using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;

namespace OpenSense.Utilities.DataWriter {
    public interface IDataWriter: IDisposable {
        void Write(object data, Envelope e);
        void Close();
    }
}
