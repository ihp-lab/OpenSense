#include "pch.h"
#include "Demuxer.h"

#include <cstring>

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

// Forward declaration for managed callback
namespace Minimp4Interop {
    int DemuxerReadManagedCallback(int64_t offset, void* buffer, size_t size, void* token);
}

// Unmanaged callback function for Demuxer
#pragma unmanaged
int DemuxerReadCallback(int64_t offset, void* buffer, size_t size, void* token) {
    return Minimp4Interop::DemuxerReadManagedCallback(offset, buffer, size, token);
}
#pragma managed

namespace Minimp4Interop {

    // Managed callback helper
    int DemuxerReadManagedCallback(int64_t offset, void* buffer, size_t size, void* token) {
        auto handle = GCHandle::FromIntPtr(IntPtr(token));
        auto demuxer = safe_cast<Demuxer^>(handle.Target);
        return demuxer->HandleRead(offset, buffer, size);
    }

#pragma region Constructor

    Demuxer::Demuxer(Stream^ stream)
        : _demux(nullptr)
        , _stream(nullptr)
        , _gcHandle()
        , _disposed(false) {

        if (stream == nullptr) {
            throw gcnew ArgumentNullException("stream");
        }

        if (!stream->CanRead) {
            throw gcnew ArgumentException("Stream must be readable", "stream");
        }

        if (!stream->CanSeek) {
            throw gcnew ArgumentException("Stream must be seekable", "stream");
        }

        _stream = stream;

        // Pin this object for callback
        _gcHandle = GCHandle::Alloc(this);

        // Create demuxer
        _demux = new MP4D_demux_t();
        memset(_demux, 0, sizeof(MP4D_demux_t));

        auto result = MP4D_open(
            _demux,
            ::DemuxerReadCallback,
            GCHandle::ToIntPtr(_gcHandle).ToPointer(),
            stream->Length
        );

        if (result == 0) {
            delete _demux;
            _demux = nullptr;
            _gcHandle.Free();
            throw gcnew InvalidOperationException("Failed to open MP4 file for demuxing");
        }
    }

#pragma endregion

#pragma region Public Methods

    unsigned int Demuxer::GetSampleCount(unsigned int trackIndex) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);
        return _demux->track[trackIndex].sample_count;
    }

    unsigned int Demuxer::GetTimescale(unsigned int trackIndex) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);
        return _demux->track[trackIndex].timescale;
    }

    unsigned int Demuxer::GetObjectType(unsigned int trackIndex) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);
        return _demux->track[trackIndex].object_type_indication;
    }

    unsigned int Demuxer::GetSampleSize(unsigned int trackIndex, unsigned int sampleIndex) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);

        if (sampleIndex >= _demux->track[trackIndex].sample_count) {
            throw gcnew ArgumentOutOfRangeException("sampleIndex");
        }

        unsigned int frameBytes = 0;
        MP4D_frame_offset(_demux, trackIndex, sampleIndex, &frameBytes, nullptr, nullptr);
        return frameBytes;
    }

    int Demuxer::ReadSample(
        unsigned int trackIndex,
        unsigned int sampleIndex,
        cli::array<Byte>^ buffer,
        [Out] unsigned int% timestamp,
        [Out] unsigned int% duration
    ) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);

        if (sampleIndex >= _demux->track[trackIndex].sample_count) {
            throw gcnew ArgumentOutOfRangeException("sampleIndex");
        }

        unsigned int frameBytes = 0;
        unsigned int ts = 0;
        unsigned int dur = 0;
        auto offset = MP4D_frame_offset(_demux, trackIndex, sampleIndex, &frameBytes, &ts, &dur);

        timestamp = ts;
        duration = dur;

        if (frameBytes == 0) {
            return 0;
        }
        if (buffer->Length < static_cast<int>(frameBytes)) {
            throw gcnew ArgumentException("Buffer too small", "buffer");
        }

        _stream->Seek(offset, SeekOrigin::Begin);

        auto totalRead = 0;
        while (totalRead < static_cast<int>(frameBytes)) {
            auto bytesRead = _stream->Read(buffer, totalRead, static_cast<int>(frameBytes) - totalRead);
            if (bytesRead == 0) {
                throw gcnew InvalidOperationException("Unexpected end of stream while reading sample");
            }
            totalRead += bytesRead;
        }

        return static_cast<int>(frameBytes);
    }

    cli::array<Byte>^ Demuxer::ReadParameterSet(unsigned int trackIndex, int index) {
        ThrowIfDisposed();
        ThrowIfInvalidTrack(trackIndex);

        if (index < 0) {
            throw gcnew ArgumentOutOfRangeException("index");
        }

        auto tr = &_demux->track[trackIndex];
        if (!tr->dsi || tr->dsi_bytes == 0) {
            return nullptr;
        }

        // Parse HEVCDecoderConfigurationRecord (ISO/IEC 14496-15)
        // Skip the fixed header:
        //   [1] configurationVersion
        //   [1] general_profile_space(2) | general_tier_flag(1) | general_profile_idc(5)
        //   [4] general_profile_compatibility_flags
        //   [6] general_constraint_indicator_flags
        //   [1] general_level_idc
        //   [2] min_spatial_segmentation_idc (with 4-bit reserved prefix)
        //   [1] parallelismType (with 6-bit reserved prefix)
        //   [1] chromaFormat (with 6-bit reserved prefix)
        //   [1] bitDepthLumaMinus8 (with 5-bit reserved prefix)
        //   [1] bitDepthChromaMinus8 (with 5-bit reserved prefix)
        //   [2] avgFrameRate
        //   [1] constantFrameRate(2) | numTemporalLayers(3) | temporalIdNested(1) | lengthSizeMinusOne(2)
        //   [1] numOfArrays
        // Total fixed header: 23 bytes

        const auto headerSize = 23;
        if (static_cast<int>(tr->dsi_bytes) < headerSize) {
            return nullptr;
        }

        auto dsi = tr->dsi;
        auto numArrays = dsi[22]; // byte at offset 22
        auto pos = headerSize;
        auto currentIndex = 0;

        for (auto arr = 0; arr < numArrays; arr++) {
            if (pos + 3 > static_cast<int>(tr->dsi_bytes)) {
                return nullptr;
            }

            // [1] array_completeness(1) | reserved(1) | NAL_unit_type(6)
            // We don't need to filter by NAL type — just iterate all arrays
            pos++; // skip array header byte

            // [2] numNalus (big-endian)
            auto numNalus = (dsi[pos] << 8) | dsi[pos + 1];
            pos += 2;

            for (auto n = 0; n < numNalus; n++) {
                if (pos + 2 > static_cast<int>(tr->dsi_bytes)) {
                    return nullptr;
                }

                // [2] nalUnitLength (big-endian)
                auto nalLength = (dsi[pos] << 8) | dsi[pos + 1];
                pos += 2;

                if (pos + nalLength > static_cast<int>(tr->dsi_bytes)) {
                    return nullptr;
                }

                if (currentIndex == index) {
                    auto nalData = gcnew cli::array<Byte>(nalLength);
                    Marshal::Copy(IntPtr(dsi + pos), nalData, 0, nalLength);
                    return nalData;
                }

                pos += nalLength;
                currentIndex++;
            }
        }

        // Index out of range
        return nullptr;
    }

#pragma endregion

#pragma region Internal Methods

    int Demuxer::HandleRead(int64_t offset, void* buffer, size_t size) {
        if (_stream == nullptr) {
            return -1;
        }

        try {
            _stream->Seek(offset, SeekOrigin::Begin);

            if (buffer != nullptr && size > 0) {
                auto data = gcnew cli::array<Byte>(static_cast<int>(size));
                auto totalRead = 0;
                while (totalRead < static_cast<int>(size)) {
                    auto bytesRead = _stream->Read(data, totalRead, static_cast<int>(size) - totalRead);
                    if (bytesRead == 0) {
                        return -1;
                    }
                    totalRead += bytesRead;
                }
                Marshal::Copy(data, 0, IntPtr(buffer), static_cast<int>(size));
            }

            return 0;
        } catch (Exception^) {
            return -1;
        }
    }

#pragma endregion

#pragma region IDisposable

    void Demuxer::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    void Demuxer::ThrowIfInvalidTrack(unsigned int trackIndex) {
        if (trackIndex >= _demux->track_count) {
            throw gcnew ArgumentOutOfRangeException("trackIndex");
        }
    }

    Demuxer::~Demuxer() {
        this->!Demuxer();
        GC::SuppressFinalize(this);
    }

    Demuxer::!Demuxer() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_demux != nullptr) {
            MP4D_close(_demux);
            delete _demux;
            _demux = nullptr;
        }

        if (_gcHandle.IsAllocated) {
            _gcHandle.Free();
        }

        // Note: We don't dispose _stream as we don't own it
        _stream = nullptr;
    }

#pragma endregion

}
