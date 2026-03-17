#include "pch.h"

#pragma managed(push, off)
#include <cstring>

#include <cstdint>
#include "NativeNalInfo.h"

// Native mirror of managed NalIndex for direct buffer access.
// Must match the layout of HMInterop::NalIndex exactly.
struct NalIndexNative {
    int32_t offset;
    int32_t length;
};

static constexpr int32_t NAL_INDEX_SIZE = sizeof(NalIndexNative);

static const unsigned char s_startCode4[] = { 0, 0, 0, 1 };
static const unsigned char s_startCode3[] = { 0, 0, 1 };

// Determine if a NAL unit requires a 4-byte start code (00 00 00 01) per Annex B rules.
// VPS, SPS, PPS, and the first NAL of an AU use 4-byte; others use 3-byte (00 00 01).
static bool NeedsLongStartCode(const unsigned char* nalData, int32_t nalSize, bool isFirst) {
    if (nalSize < 2) {
        return true;
    }
    return HevcNeedsLongStartCode(nalData, isFirst);
}

// Calculate total buffer size for NalIndex header + Annex B data
static int32_t CalcBufferSize(const NativeNalInfo* nals, int32_t count, int32_t nalIndexSize) {
    auto annexBSize = int32_t(0);
    for (auto i = int32_t(0); i < count; i++) {
        annexBSize += (nals[i].longStartCode ? 4 : 3) + nals[i].size;
    }
    return count * nalIndexSize + annexBSize;
}

// Assemble NalIndex header + Annex B data into pre-allocated buffer
static void AssembleFromNativeNals(unsigned char* buffer, const NativeNalInfo* nals, int32_t count, int32_t nalIndexSize) {
    auto headerPtr = buffer;
    auto dataPtr = buffer + count * nalIndexSize;

    for (auto i = int32_t(0); i < count; i++) {
        auto startCodeLen = nals[i].longStartCode ? 4 : 3;
        auto startCode = nals[i].longStartCode ? s_startCode4 : s_startCode3;

        memcpy(dataPtr, startCode, startCodeLen);
        dataPtr += startCodeLen;

        NalIndexNative entry = { static_cast<int32_t>(dataPtr - buffer), nals[i].size };
        memcpy(headerPtr, &entry, nalIndexSize);
        headerPtr += nalIndexSize;

        memcpy(dataPtr, nals[i].data, nals[i].size);
        dataPtr += nals[i].size;
    }
}

// Parse length-prefixed NALs: count them and calculate Annex B size
static int32_t CountLengthPrefixedNals(const unsigned char* data, int32_t dataLen, int32_t* annexBSize) {
    auto count = int32_t(0);
    *annexBSize = 0;
    auto offset = int32_t(0);
    while (offset + 4 <= dataLen) {
        auto nalSize = (data[offset] << 24) | (data[offset + 1] << 16)
            | (data[offset + 2] << 8) | data[offset + 3];
        offset += 4;
        if (nalSize <= 0 || offset + nalSize > dataLen) {
            break;
        }
        auto longSC = NeedsLongStartCode(data + offset, nalSize, count == 0);
        *annexBSize += (longSC ? 4 : 3) + nalSize;
        count++;
        offset += nalSize;
    }
    return count;
}

// Convert length-prefixed NALs to NalIndex header + Annex B data
static void AssembleFromLengthPrefixed(unsigned char* buffer, const unsigned char* data, int32_t dataLen, int32_t nalCount, int32_t nalIndexSize) {
    auto headerPtr = buffer;
    auto dataPtr = buffer + nalCount * nalIndexSize;
    auto offset = int32_t(0);
    auto nalIdx = int32_t(0);

    while (offset + 4 <= dataLen && nalIdx < nalCount) {
        auto nalSize = (data[offset] << 24) | (data[offset + 1] << 16)
            | (data[offset + 2] << 8) | data[offset + 3];
        offset += 4;
        if (nalSize <= 0 || offset + nalSize > dataLen) {
            break;
        }

        auto longSC = NeedsLongStartCode(data + offset, nalSize, nalIdx == 0);
        auto startCodeLen = longSC ? 4 : 3;
        auto startCode = longSC ? s_startCode4 : s_startCode3;

        memcpy(dataPtr, startCode, startCodeLen);
        dataPtr += startCodeLen;

        NalIndexNative entry = { static_cast<int32_t>(dataPtr - buffer), nalSize };
        memcpy(headerPtr, &entry, nalIndexSize);
        headerPtr += nalIndexSize;

        memcpy(dataPtr, data + offset, nalSize);
        dataPtr += nalSize;

        offset += nalSize;
        nalIdx++;
    }
}

// Scan Annex B data: count NALs and build NalIndex entries into the header region.
// The data is copied as-is (already in Annex B format); only the NalIndex header is prepended.
static int32_t ScanAnnexBNals(const unsigned char* data, int32_t dataLen, NalIndexNative* entries, int32_t maxEntries) {
    auto count = int32_t(0);
    auto i = int32_t(0);

    while (i < dataLen) {
        // Find start code (00 00 01 or 00 00 00 01)
        if (i + 3 <= dataLen && data[i] == 0 && data[i + 1] == 0) {
            int startCodeLen;
            if (data[i + 2] == 1) {
                startCodeLen = 3;
            } else if (i + 4 <= dataLen && data[i + 2] == 0 && data[i + 3] == 1) {
                startCodeLen = 4;
            } else {
                i++;
                continue;
            }

            auto nalStart = i + startCodeLen;

            // Find end of this NAL (next start code or end of data)
            auto nalEnd = dataLen;
            for (auto j = int32_t(nalStart); j + 2 < dataLen; j++) {
                if (data[j] == 0 && data[j + 1] == 0
                    && (data[j + 2] == 1 || (j + 3 < dataLen && data[j + 2] == 0 && data[j + 3] == 1))) {
                    nalEnd = j;
                    break;
                }
            }

            if (count < maxEntries && entries != nullptr) {
                entries[count] = { nalStart, nalEnd - nalStart };
            }
            count++;
            i = nalEnd;
        } else {
            i++;
        }
    }
    return count;
}

#pragma managed(pop)

#include "AccessUnit.h"

using namespace System;
using namespace System::Buffers;

namespace HMInterop {

#pragma region Classes

    ref class AccessUnitEnumerator : System::Collections::Generic::IEnumerator<ReadOnlyMemory<Byte>> {
    private:
        AccessUnit^ _au;
        int _index;

    public:
        AccessUnitEnumerator(AccessUnit^ au) : _au(au), _index(-1) {}

        property ReadOnlyMemory<Byte> Current {
            virtual ReadOnlyMemory<Byte> get() {
                return _au[_index];
            }
        }

        property Object^ CurrentObj {
            virtual Object^ get() = System::Collections::IEnumerator::Current::get{
                return _au[_index];
            }
        }

        virtual bool MoveNext() {
            _index++;
            return _index < _au->Count;
        }

        virtual void Reset() {
            _index = -1;
        }

        virtual ~AccessUnitEnumerator() {}
    };

#pragma endregion

    AccessUnit::AccessUnit(IMemoryOwner<Byte>^ owner, int length, int count, long long pts, long long dts)
        : _owner(owner)
        , _length(length)
        , _count(count)
        , _pts(pts)
        , _dts(dts)
        , _disposed(false) {}

#pragma region Factory Methods

    AccessUnit^ AccessUnit::CreateFromNativeNals(void* nalsPtr, int count, long long pts, long long dts) {
        auto nals = static_cast<NativeNalInfo*>(nalsPtr);
        auto nalIndexSize = NAL_INDEX_SIZE;
        auto totalSize = CalcBufferSize(nals, count, nalIndexSize);

        auto owner = MemoryPool<Byte>::Shared->Rent(totalSize);
        auto handle = owner->Memory.Pin();
        AssembleFromNativeNals(static_cast<unsigned char*>(handle.Pointer), nals, count, nalIndexSize);
        delete safe_cast<IDisposable^>(handle);

        return gcnew AccessUnit(owner, totalSize, count, pts, dts);
    }

    AccessUnit^ AccessUnit::CreateFromAnnexB(ReadOnlyMemory<Byte> annexBData, long long pts, long long dts) {
        auto srcLen = annexBData.Length;

        // First pass: count NALs (need pin for native scan function)
        int nalCount;
        {
            auto srcHandle = annexBData.Pin();
            nalCount = ScanAnnexBNals(static_cast<const unsigned char*>(srcHandle.Pointer), srcLen, nullptr, 0);
            delete safe_cast<IDisposable^>(srcHandle);
        }

        auto headerSize = nalCount * NAL_INDEX_SIZE;
        auto totalSize = headerSize + srcLen;

        // Rent buffer: [NalIndex header] [Annex B data copied as-is]
        auto owner = MemoryPool<Byte>::Shared->Rent(totalSize);

        // Copy Annex B data and build NalIndex entries in a single pin
        {
            auto srcHandle = annexBData.Pin();
            auto dstHandle = owner->Memory.Pin();
            auto srcPtr = static_cast<const unsigned char*>(srcHandle.Pointer);
            auto buffer = static_cast<unsigned char*>(dstHandle.Pointer);

            // Copy Annex B data after header
            memcpy(buffer + headerSize, srcPtr, srcLen);

            // Build NalIndex entries from the copied data (offsets adjusted for header)
            auto entries = reinterpret_cast<NalIndexNative*>(buffer);
            ScanAnnexBNals(buffer + headerSize, srcLen, entries, nalCount);
            for (auto i = int32_t(0); i < nalCount; i++) {
                entries[i].offset += headerSize;
            }

            delete safe_cast<IDisposable^>(dstHandle);
            delete safe_cast<IDisposable^>(srcHandle);
        }

        return gcnew AccessUnit(owner, totalSize, nalCount, pts, dts);
    }

    AccessUnit^ AccessUnit::CreateFromLengthPrefixed(ReadOnlyMemory<Byte> mp4Sample, long long pts, long long dts) {
        auto sampleHandle = mp4Sample.Pin();
        auto samplePtr = static_cast<const unsigned char*>(sampleHandle.Pointer);
        auto sampleLen = mp4Sample.Length;

        auto annexBSize = int32_t(0);
        auto nalCount = CountLengthPrefixedNals(samplePtr, sampleLen, &annexBSize);
        auto nalIndexSize = NAL_INDEX_SIZE;
        auto totalSize = nalCount * nalIndexSize + annexBSize;

        auto owner = MemoryPool<Byte>::Shared->Rent(totalSize);
        auto writeHandle = owner->Memory.Pin();
        AssembleFromLengthPrefixed(static_cast<unsigned char*>(writeHandle.Pointer), samplePtr, sampleLen, nalCount, nalIndexSize);
        delete safe_cast<IDisposable^>(writeHandle);
        delete safe_cast<IDisposable^>(sampleHandle);

        return gcnew AccessUnit(owner, totalSize, nalCount, pts, dts);
    }

#pragma endregion

#pragma region Properties

    long long AccessUnit::PresentationTimeOffset::get() {
        ThrowIfDisposed();
        return _pts;
    }

    long long AccessUnit::DecodingTimeOffset::get() {
        ThrowIfDisposed();
        return _dts;
    }

    ReadOnlyMemory<Byte> AccessUnit::AnnexB::get() {
        ThrowIfDisposed();
        auto headerSize = _count * NAL_INDEX_SIZE;
        return _owner->Memory.Slice(headerSize, _length - headerSize);
    }

    void* AccessUnit::PinBuffer() {
        ThrowIfDisposed();
        if (_pinHandle.Pointer != nullptr) {
            throw gcnew InvalidOperationException("Buffer is already pinned.");
        }
        _pinHandle = _owner->Memory.Pin();
        return _pinHandle.Pointer;
    }

    void AccessUnit::UnpinBuffer() {
        // Boxing _pinHandle creates a copy; Dispose on the copy unpins (frees GCHandle)
        // but does NOT reset the original field's Pointer. Must reset manually.
        delete safe_cast<IDisposable^>(_pinHandle);
        _pinHandle = MemoryHandle();
    }

    void AccessUnit::GetNalFromPinnedBuffer(const void* pinnedBuffer, int index, const unsigned char*& nalData, int& nalLength) {
        auto buffer = static_cast<const unsigned char*>(pinnedBuffer);
        NalIndexNative entry;
        memcpy(&entry, buffer + index * NAL_INDEX_SIZE, NAL_INDEX_SIZE);
        nalData = buffer + entry.offset;
        nalLength = entry.length;
    }

#pragma endregion

#pragma region IReadOnlyList

    int AccessUnit::Count::get() {
        ThrowIfDisposed();
        return _count;
    }

    ReadOnlyMemory<Byte> AccessUnit::default::get(int index) {
        ThrowIfDisposed();
        if (index < 0 || index >= _count) {
            throw gcnew ArgumentOutOfRangeException("index");
        }
        // Use independent pin (not PinBuffer) so the indexer works regardless of external pin state
        auto handle = _owner->Memory.Pin();
        const unsigned char* nalData;
        int nalLength;
        GetNalFromPinnedBuffer(handle.Pointer, index, nalData, nalLength);
        auto offset = static_cast<int>(nalData - static_cast<const unsigned char*>(handle.Pointer));
        delete safe_cast<IDisposable^>(handle);
        return _owner->Memory.Slice(offset, nalLength);
    }

    System::Collections::Generic::IEnumerator<ReadOnlyMemory<Byte>>^ AccessUnit::GetEnumerator() {
        ThrowIfDisposed();
        return gcnew AccessUnitEnumerator(this);
    }

    System::Collections::IEnumerator^ AccessUnit::GetEnumeratorNonGeneric() {
        return GetEnumerator();
    }

#pragma endregion

#pragma region IDisposable

    void AccessUnit::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    AccessUnit::~AccessUnit() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        auto wasPinned = _pinHandle.Pointer != nullptr;
        if (wasPinned) {
            UnpinBuffer();
        }

        this->!AccessUnit();

        if (wasPinned) {
            throw gcnew InvalidOperationException("AccessUnit was disposed while buffer was still pinned. Call UnpinBuffer() before disposing.");
        }
    }

    AccessUnit::!AccessUnit() {
        if (_owner == nullptr) {
            return;
        }
        delete _owner;
        _owner = nullptr;
    }

#pragma endregion
}
