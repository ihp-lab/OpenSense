using System;
using Microsoft.Psi;

namespace OpenSense.DataWriter.Contracts {
    public interface IDataWriter<T>: IDisposable {
        void Write(T data, Envelope e);
    }
}
