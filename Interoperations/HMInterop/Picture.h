#pragma once

#include "PictureYuv.h"
#include "SequenceParameterSet.h"

using namespace System;

namespace HMInterop {
    /// <summary>
    /// Immutable snapshot of a decoded HEVC picture.
    /// Contains pixel data (PictureYuv) and stream metadata (SequenceParameterSet, POC).
    /// Dispose behavior for PictureYuv is determined by the ownership parameter.
    /// </summary>
    public ref class Picture sealed : IDisposable {
    private:
        PictureYuv^ _picYuv;
        int _poc;
        SequenceParameterSet^ _sps;
        PictureYuvOwnership _ownership;

    public:
        /// <summary>
        /// Create a Picture wrapping the provided PictureYuv.
        /// </summary>
        /// <param name="picYuv">Pixel data</param>
        /// <param name="poc">Picture order count</param>
        /// <param name="sps">Sequence parameter set metadata</param>
        /// <param name="ownership">Determines dispose behavior for picYuv:
        /// None = do not dispose, Owned = destroy, Pooled = return to PictureYuvPool</param>
        Picture(PictureYuv^ picYuv, int poc, SequenceParameterSet^ sps, PictureYuvOwnership ownership);

        property PictureYuv^ PicYuv {
            PictureYuv^ get();
        }

        property int POC {
            int get() { return _poc; }
        }

        property SequenceParameterSet^ Sps {
            SequenceParameterSet^ get() { return _sps; }
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~Picture();
        !Picture();
#pragma endregion
    };
}
