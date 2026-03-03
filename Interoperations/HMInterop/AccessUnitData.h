#pragma once

#include <cstdint>

using namespace System;

namespace HMInterop {
    /// <summary>
    /// Holds encoded NAL data from an access unit in Annex B format.
    /// Owns native memory that is freed on disposal.
    /// </summary>
    public ref class AccessUnitData : IDisposable {
    private:
        uint8_t* _data;
        int _length;
        long long _pts;
        int _poc;

    internal:
        /// <summary>
        /// Creates an AccessUnitData from a native buffer.
        /// Takes ownership of the data pointer.
        /// </summary>
        AccessUnitData(uint8_t* data, int length, long long pts, int poc);

    public:
        /// <summary>
        /// Gets the pointer to the Annex B encoded data
        /// </summary>
        property IntPtr Data {
            IntPtr get() {
                ThrowIfDisposed();
                return IntPtr(_data);
            }
        }

        /// <summary>
        /// Gets the length of the encoded data in bytes
        /// </summary>
        property int Length {
            int get() {
                ThrowIfDisposed();
                return _length;
            }
        }

        /// <summary>
        /// Gets the presentation timestamp
        /// </summary>
        property long long PTS {
            long long get() {
                ThrowIfDisposed();
                return _pts;
            }
        }

        /// <summary>
        /// Gets the picture order count
        /// </summary>
        property int POC {
            int get() {
                ThrowIfDisposed();
                return _poc;
            }
        }

#pragma region IDisposable
    private:
        bool _disposed;

        void ThrowIfDisposed();

    public:
        ~AccessUnitData();
        !AccessUnitData();
#pragma endregion
    };
}
