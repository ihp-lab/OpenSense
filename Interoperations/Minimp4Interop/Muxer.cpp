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

    Muxer::Muxer(String^ filename)
        : Muxer(filename, MuxMode::Default) {}

    Muxer::Muxer(String^ filename, MuxMode mode)
        : _mux(nullptr)
        , _fileStream(nullptr)
        , _disposed(false)
        , _mode(mode)
        , _tracks(gcnew Dictionary<int, Track^>()) {

        if (String::IsNullOrEmpty(filename)) {
            throw gcnew ArgumentNullException("filename");
        }

        // Open file stream
        try {
            _fileStream = gcnew FileStream(filename, FileMode::Create, FileAccess::Write);
        } catch (Exception^ ex) {
            throw gcnew InvalidOperationException("Failed to create output file: " + ex->Message, ex);
        }

        // Pin this object for callback
        _gcHandle = GCHandle::Alloc(this);

        // Create muxer
        int sequential = (mode == MuxMode::Sequential) ? 1 : 0;
        int fragmented = (mode == MuxMode::Fragmented) ? 1 : 0;

        // Use unmanaged callback function defined above
        _mux = MP4E_open(sequential, fragmented, GCHandle::ToIntPtr(_gcHandle).ToPointer(), ::MuxerWriteCallback);

        if (_mux == nullptr) {
            _gcHandle.Free();
            delete _fileStream;
            throw gcnew InvalidOperationException("Failed to create MP4 muxer");
        }
    }

    void Muxer::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Muxer::~Muxer() {
        if (_disposed) {
            return;
        }

        this->!Muxer();
        _disposed = true;
    }

    Muxer::!Muxer() {
        if (_mux != nullptr) {
            MP4E_close(_mux);
            _mux = nullptr;
        }

        if (_gcHandle.IsAllocated) {
            _gcHandle.Free();
        }

        if (_fileStream != nullptr) {
            _fileStream->Close();
            delete _fileStream;
            _fileStream = nullptr;
        }

        if (_tracks != nullptr) {
            _tracks->Clear();
            _tracks = nullptr;
        }
    }

    int Muxer::HandleWrite(int64_t offset, const void* buffer, size_t size) {
        if (_fileStream == nullptr) {
            return -1;
        }

        try {
            // Seek to offset
            _fileStream->Seek(offset, SeekOrigin::Begin);

            // Write data
            if (buffer != nullptr && size > 0) {
                array<Byte>^ data = gcnew array<Byte>(static_cast<int>(size));
                Marshal::Copy(IntPtr(const_cast<void*>(buffer)), data, 0, static_cast<int>(size));
                _fileStream->Write(data, 0, data->Length);
            }

            return 0;
        } catch (Exception^) {
            return -1;
        }
    }

    int Muxer::AddTrack(Track^ track) {
        ThrowIfDisposed();

        if (track == nullptr) {
            throw gcnew ArgumentNullException("track");
        }

        if (IsClosed) {
            throw gcnew InvalidOperationException("Cannot add tracks after closing");
        }

        MP4E_track_t* nativeTrack = track->InternalTrack;
        int trackId = MP4E_add_track(_mux, nativeTrack);

        if (trackId < 0) {
            throw gcnew InvalidOperationException("Failed to add track to MP4 file");
        }

        _tracks[trackId] = track;
        return trackId;
    }

    void Muxer::PutSample(int trackId, array<Byte>^ data, int duration, SampleType sampleType) {
        if (data == nullptr) {
            throw gcnew ArgumentNullException("data");
        }

        pin_ptr<Byte> pinnedData = &data[0];
        PutSample(trackId, IntPtr(pinnedData), data->Length, duration, sampleType);
    }

    void Muxer::PutSample(int trackId, IntPtr data, int size, int duration, SampleType sampleType) {
        ThrowIfDisposed();

        if (IsClosed) {
            throw gcnew InvalidOperationException("Cannot write samples after closing");
        }

        if (!_tracks->ContainsKey(trackId)) {
            throw gcnew ArgumentException("Invalid track ID: " + trackId);
        }

        if (data == IntPtr::Zero || size <= 0) {
            throw gcnew ArgumentException("Invalid sample data");
        }

        int result = MP4E_put_sample(_mux, trackId,
                                  data.ToPointer(), size, duration, static_cast<int>(sampleType));

        if (result != 0) {
            throw gcnew InvalidOperationException("Failed to write sample to MP4 file");
        }
    }

    void Muxer::Close() {
        ThrowIfDisposed();

        if (_mux != nullptr && !IsClosed) {
            MP4E_close(_mux);
            _mux = nullptr;
        }

        if (_fileStream != nullptr) {
            _fileStream->Flush();
            _fileStream->Close();
            delete _fileStream;
            _fileStream = nullptr;
        }
    }

}