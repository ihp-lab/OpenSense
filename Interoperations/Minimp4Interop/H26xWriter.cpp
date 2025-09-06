#include "pch.h"
#include "H26xWriter.h"

#include <vcclr.h>
#include <cstring>

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

// Forward declaration for managed callback
namespace Minimp4Interop {
    int H26xWriterWriteManagedCallback(int64_t offset, const void* buffer, size_t size, void* token);
}

// Unmanaged callback function for H26xWriter
#pragma unmanaged
int H26xWriterCallback(int64_t offset, const void* buffer, size_t size, void* token) {
    // Call the managed callback helper
    return Minimp4Interop::H26xWriterWriteManagedCallback(offset, buffer, size, token);
}
#pragma managed

namespace Minimp4Interop {
    // Managed callback helper
    int H26xWriterWriteManagedCallback(int64_t offset, const void* buffer, size_t size, void* token) {
        GCHandle handle = GCHandle::FromIntPtr(IntPtr(token));
        H26xWriter^ writer = safe_cast<H26xWriter^>(handle.Target);
        return writer->HandleWrite(offset, buffer, size);
    }

    H26xWriter::H26xWriter(String^ filename, int width, int height, bool isHevc)
        : H26xWriter(filename, width, height, 30.0, isHevc) {}

    H26xWriter::H26xWriter(String^ filename, int width, int height, double frameRate, bool isHevc)
        : _writer(nullptr)
        , _fileStream(nullptr)
        , _disposed(false)
        , _isHevc(isHevc)
        , _width(width)
        , _height(height) {

        if (String::IsNullOrEmpty(filename)) {
            throw gcnew ArgumentNullException("filename");
        }

        if (width <= 0 || height <= 0) {
            throw gcnew ArgumentException("Width and height must be positive");
        }

        if (frameRate <= 0) {
            throw gcnew ArgumentException("Frame rate must be positive");
        }

        // Open file stream
        try {
            _fileStream = gcnew FileStream(filename, FileMode::Create, FileAccess::Write);
        } catch (Exception^ ex) {
            throw gcnew InvalidOperationException("Failed to create output file: " + ex->Message, ex);
        }

        // Pin this object for callback
        _gcHandle = GCHandle::Alloc(this);

        // Create writer
        _writer = new mp4_h26x_writer_t();
        memset(_writer, 0, sizeof(mp4_h26x_writer_t));

        // Initialize writer without mux first
        int result = mp4_h26x_write_init(_writer, nullptr, width, height, isHevc ? 1 : 0);

        if (result < 0) {
            delete _writer;
            _gcHandle.Free();
            delete _fileStream;
            throw gcnew InvalidOperationException("Failed to initialize H26x writer");
        }

        // Open muxer with sequential mode for streaming
        // Use unmanaged callback function defined above
        _writer->mux = MP4E_open(1, 0, GCHandle::ToIntPtr(_gcHandle).ToPointer(),
                                ::H26xWriterCallback);

        if (_writer->mux == nullptr) {
            delete _writer;
            _gcHandle.Free();
            delete _fileStream;
            throw gcnew InvalidOperationException("Failed to create MP4 muxer");
        }

        // Re-initialize with actual muxer
        result = mp4_h26x_write_init(_writer, _writer->mux, width, height, isHevc ? 1 : 0);

        if (result < 0) {
            MP4E_close(_writer->mux);
            delete _writer;
            _gcHandle.Free();
            delete _fileStream;
            throw gcnew InvalidOperationException("Failed to initialize H26x writer with muxer");
        }
    }

    void H26xWriter::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    H26xWriter::~H26xWriter() {
        if (_disposed) {
            return;
        }

        this->!H26xWriter();
        _disposed = true;
    }

    H26xWriter::!H26xWriter() {
        if (_writer != nullptr) {
            if (_writer->mux != nullptr) {
                MP4E_close(_writer->mux);
                _writer->mux = nullptr;
            }

            delete _writer;
            _writer = nullptr;
        }

        if (_gcHandle.IsAllocated) {
            _gcHandle.Free();
        }

        if (_fileStream != nullptr) {
            _fileStream->Close();
            delete _fileStream;
            _fileStream = nullptr;
        }
    }

    int H26xWriter::HandleWrite(int64_t offset, const void* buffer, size_t size) {
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

    void H26xWriter::WriteNal(array<Byte>^ nalData, long long timestamp) {
        if (nalData == nullptr) {
            throw gcnew ArgumentNullException("nalData");
        }

        pin_ptr<Byte> pinnedData = &nalData[0];
        WriteNal(IntPtr(pinnedData), nalData->Length, timestamp);
    }

    void H26xWriter::WriteNal(IntPtr nalData, int size, long long timestamp) {
        ThrowIfDisposed();

        if (IsClosed) {
            throw gcnew InvalidOperationException("Cannot write NAL units after closing");
        }

        if (nalData == IntPtr::Zero || size <= 0) {
            throw gcnew ArgumentException("Invalid NAL data");
        }

        int result = mp4_h26x_write_nal(_writer, static_cast<const uint8_t*>(nalData.ToPointer()),
                                         size, static_cast<unsigned>(timestamp));

        if (result < 0) {
            throw gcnew InvalidOperationException("Failed to write NAL unit to MP4 file");
        }
    }

    void H26xWriter::Close() {
        ThrowIfDisposed();

        if (_writer != nullptr && !IsClosed) {
            if (_writer->mux != nullptr) {
                MP4E_close(_writer->mux);
                _writer->mux = nullptr;
            }
        }

        if (_fileStream != nullptr) {
            _fileStream->Flush();
            _fileStream->Close();
            delete _fileStream;
            _fileStream = nullptr;
        }
    }

}