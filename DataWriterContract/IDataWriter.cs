using System;
using Microsoft.Psi;

namespace OpenSense.DataWriter.Contract {
    public interface IDataWriter<T>: IDisposable {
        void Write(T data, Envelope e);
    }
}
