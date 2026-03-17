#pragma once

using namespace System;
using namespace System::Buffers;
using namespace System::Collections::Generic;

namespace HMInterop {
    /// <summary>
    /// Holds encoded NAL data for one video frame (Access Unit).
    /// Buffer layout: [NalIndex header] [Annex B data].
    /// Indexer returns raw NAL bytes without start codes.
    /// AnnexB property returns the full Annex B stream.
    /// Backed by pooled memory; disposing returns the buffer to the pool.
    /// </summary>
    public ref class AccessUnit sealed : IReadOnlyList<ReadOnlyMemory<Byte>>, IDisposable {
    private:
        // Buffer layout: [Count * sizeof(NalIndex)] [Annex B data]
        // NalIndex entries point into the Annex B region (past start codes).
        IMemoryOwner<Byte>^ _owner;
        int _length;
        int _count;
        long long _pts;
        long long _dts;
        MemoryHandle _pinHandle;

    private:
        AccessUnit(IMemoryOwner<Byte>^ owner, int length, int count, long long pts, long long dts);

    internal:
        /// <summary>
        /// Create from native NAL pointers (encoder output).
        /// Rents buffer, writes NalIndex header + start codes + NAL data. 2 copies total.
        /// </summary>
        static AccessUnit^ CreateFromNativeNals(void* nals, int count, long long pts, long long dts);

    public:
        /// <summary>
        /// Create from Annex B data (e.g. deserialization).
        /// Scans start codes to build NAL index. 1 copy total.
        /// </summary>
        static AccessUnit^ CreateFromAnnexB(ReadOnlyMemory<Byte> annexBData, long long pts, long long dts);

        /// <summary>
        /// Create from length-prefixed NAL data (MP4 sample).
        /// Converts to Annex B internally. 1 copy total.
        /// </summary>
        static AccessUnit^ CreateFromLengthPrefixed(ReadOnlyMemory<Byte> mp4Sample, long long pts, long long dts);

        /// <summary>
        /// Presentation (display) time offset relative to the first frame, in .NET Ticks.
        /// Not part of the standard Access Unit; added for pipeline timing coordination.
        /// Analogous to PTS in container formats.
        /// </summary>
        property long long PresentationTimeOffset {
            long long get();
        }

        /// <summary>
        /// Decoding time offset relative to the first frame, in .NET Ticks.
        /// Not part of the standard Access Unit; added for pipeline timing coordination.
        /// By design, this should be consistent with the Psi Envelope's OriginatingTime
        /// (which carries the same value as an absolute DateTime).
        /// Analogous to DTS in container formats.
        /// </summary>
        property long long DecodingTimeOffset {
            long long get();
        }

        /// <summary>
        /// Get the entire Access Unit in Annex B format (with start codes). Zero-copy.
        /// </summary>
        property ReadOnlyMemory<Byte> AnnexB {
            ReadOnlyMemory<Byte> get();
        }

    internal:
        /// <summary>
        /// Pin the internal buffer for native access. Must be paired with UnpinBuffer().
        /// Returns a pointer for use with GetNalFromPinnedBuffer().
        /// </summary>
        void* PinBuffer();

        /// <summary>
        /// Unpin the internal buffer previously pinned by PinBuffer().
        /// </summary>
        void UnpinBuffer();

        /// <summary>
        /// Read NAL data pointer and length for the given index from a pinned buffer.
        /// The pinnedBuffer must be the pointer returned by PinBuffer().
        /// </summary>
        static void GetNalFromPinnedBuffer(const void* pinnedBuffer, int index, const unsigned char*& nalData, int& nalLength);

#pragma region IReadOnlyList
    public:
        property int Count {
            virtual int get();
        }

        /// <summary>
        /// Get raw NAL bytes at the given index (without Annex B start code).
        /// </summary>
        property ReadOnlyMemory<Byte> default[int] {
            virtual ReadOnlyMemory<Byte> get(int index);
        }

        virtual System::Collections::Generic::IEnumerator<ReadOnlyMemory<Byte>>^ GetEnumerator();

    private:
        virtual System::Collections::IEnumerator^ GetEnumeratorNonGeneric()
            = System::Collections::IEnumerable::GetEnumerator;
#pragma endregion

#pragma region IDisposable
    private:
        bool _disposed;
        void ThrowIfDisposed();

    public:
        ~AccessUnit();
        !AccessUnit();
#pragma endregion
    };
}
