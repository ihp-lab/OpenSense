#pragma once

using namespace System;
using namespace System::Buffers;

namespace HMInterop {
    /// <summary>
    /// Holds encoded NAL data from an access unit in Annex B format.
    /// Backed by pooled memory from MemoryPool. Disposing returns the buffer to the pool.
    /// </summary>
    public ref class AccessUnitData : IDisposable {
    private:
        IMemoryOwner<Byte>^ _owner;
        int _length;
        long long _pts;
        int _poc;

    internal:
        /// <summary>
        /// Creates an AccessUnitData from a pooled memory owner.
        /// Takes ownership of the IMemoryOwner.
        /// </summary>
        AccessUnitData(IMemoryOwner<Byte>^ owner, int length, long long pts, int poc);

    public:
        /// <summary>
        /// Gets the encoded data as a read-only memory slice of the exact length
        /// </summary>
        property ReadOnlyMemory<Byte> Data {
            ReadOnlyMemory<Byte> get() {
                ThrowIfDisposed();
                return _owner->Memory.Slice(0, _length);
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
