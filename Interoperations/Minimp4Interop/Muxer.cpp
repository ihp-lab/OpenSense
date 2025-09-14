#include "pch.h"
#include "Muxer.h"

#include <vcclr.h>
#include <cstring>

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

// Forward declaration for managed callback
namespace Minimp4Interop {
    int MuxerWriteManagedCallback(int64_t offset, const void* buffer, size_t size, void* token);
}

// Unmanaged callback function for Muxer
#pragma unmanaged
int MuxerWriteCallback(int64_t offset, const void* buffer, size_t size, void* token) {
    // Call the managed callback helper
    return Minimp4Interop::MuxerWriteManagedCallback(offset, buffer, size, token);
}
#pragma managed

namespace Minimp4Interop {

    // Managed callback helper
    int MuxerWriteManagedCallback(int64_t offset, const void* buffer, size_t size, void* token) {
        GCHandle handle = GCHandle::FromIntPtr(IntPtr(token));
        Muxer^ muxer = safe_cast<Muxer^>(handle.Target);
        return muxer->HandleWrite(offset, buffer, size);
    }

#pragma region Constructor

    Muxer::Muxer(Stream^ stream, MuxMode mode)
        : _mux(nullptr)
        , _stream(nullptr)
        , _gcHandle()
        , _mode(mode)
        , _disposed(false) {

        if (stream == nullptr) {
            throw gcnew ArgumentNullException("stream");
        }

        if (!stream->CanWrite) {
            throw gcnew ArgumentException("Stream must be writable", "stream");
        }

        // Check if stream needs to be seekable based on mode
        auto needsSeek = (mode != MuxMode::Sequential && mode != MuxMode::Fragmented);
        if (needsSeek && !stream->CanSeek) {
            throw gcnew ArgumentException("Stream must be seekable for non-sequential modes", "stream");
        }

        _stream = stream;

        // Pin this object for callback
        _gcHandle = GCHandle::Alloc(this);

        // Create muxer
        auto sequential = (mode == MuxMode::Sequential) ? 1 : 0;
        auto fragmented = (mode == MuxMode::Fragmented) ? 1 : 0;

        // Use unmanaged callback function defined above
        _mux = MP4E_open(sequential, fragmented, GCHandle::ToIntPtr(_gcHandle).ToPointer(), ::MuxerWriteCallback);

        if (_mux == nullptr) {
            _gcHandle.Free();
            throw gcnew InvalidOperationException("Failed to create MP4 muxer");
        }
    }

#pragma endregion

#pragma region Public Methods

    int Muxer::AddTrack(Track^ track) {
        ThrowIfDisposed();

        if (track == nullptr) {
            throw gcnew ArgumentNullException("track");
        }

        auto nativeTrack = track->InternalTrack;
        auto trackId = MP4E_add_track(_mux, nativeTrack);

        if (trackId < 0) {
            throw gcnew InvalidOperationException("Failed to add track to MP4 file");
        }

        return trackId;
    }

    void Muxer::PutSample(int trackId, IntPtr data, int size, int duration, SampleType sampleType) {
        ThrowIfDisposed();

        if (data == IntPtr::Zero || size <= 0) {
            throw gcnew ArgumentException("Invalid sample data");
        }

        auto result = MP4E_put_sample(_mux, trackId,
                                  data.ToPointer(), size, duration, static_cast<int>(sampleType));

        if (result != 0) {
            throw gcnew InvalidOperationException("Failed to write sample to MP4 file");
        }
    }

#pragma endregion

#pragma region Internal Methods

    int Muxer::HandleWrite(int64_t offset, const void* buffer, size_t size) {
        if (_stream == nullptr) {
            return -1;
        }

        try {
            // Seek to offset if stream supports it
            if (_stream->CanSeek) {
                _stream->Seek(offset, SeekOrigin::Begin);
            }

            // Write data
            if (buffer != nullptr && size > 0) {
                auto data = gcnew array<Byte>(static_cast<int>(size));
                Marshal::Copy(IntPtr(const_cast<void*>(buffer)), data, 0, static_cast<int>(size));
                _stream->Write(data, 0, data->Length);
            }

            return 0;
        } catch (Exception^) {
            return -1;
        }
    }

#pragma endregion

#pragma region IDisposable

    void Muxer::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Muxer::~Muxer() {
        this->!Muxer();
        GC::SuppressFinalize(this);
    }

    Muxer::!Muxer() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        // Close muxer and write final metadata
        if (_mux != nullptr) {
            MP4E_close(_mux);
            _mux = nullptr;
        }

        if (_gcHandle.IsAllocated) {
            _gcHandle.Free();
        }

        if (_stream != nullptr) {
            _stream->Flush();
            // Note: We don't delete _stream as we don't own it
            _stream = nullptr;
        }
    }

#pragma endregion

}