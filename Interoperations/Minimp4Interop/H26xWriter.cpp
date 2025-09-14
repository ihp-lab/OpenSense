#include "pch.h"
#include "H26xWriter.h"

#include <vcclr.h>
#include <cstring>

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;

namespace Minimp4Interop {

#pragma region Constructor

    H26xWriter::H26xWriter(Muxer^ muxer, int width, int height, bool isHEVC)
        : _writer(nullptr)
        , _muxer(muxer)
        , _isHEVC(isHEVC)
        , _width(width)
        , _height(height)
        , _disposed(false) {

        if (muxer == nullptr) {
            throw gcnew ArgumentNullException("muxer");
        }

        if (width <= 0 || height <= 0) {
            throw gcnew ArgumentException("Width and height must be positive");
        }

        // Create writer
        _writer = new mp4_h26x_writer_t();
        memset(_writer, 0, sizeof(mp4_h26x_writer_t));

        // Initialize writer with muxer's internal MP4E_mux_t
        MP4E_mux_t* internalMux = _muxer->InternalMux;
        auto result = mp4_h26x_write_init(_writer, internalMux, width, height, isHEVC ? 1 : 0);

        if (result < 0) {
            delete _writer;
            throw gcnew InvalidOperationException("Failed to initialize H26x writer");
        }
    }

#pragma endregion

#pragma region Public Methods

    void H26xWriter::WriteNal(IntPtr nalData, int size, unsigned int timestamp90kHz) {
        ThrowIfDisposed();

        if (nalData == IntPtr::Zero || size <= 0) {
            throw gcnew ArgumentException("Invalid NAL data");
        }

        auto result = mp4_h26x_write_nal(_writer, static_cast<const uint8_t*>(nalData.ToPointer()), size, timestamp90kHz);

        if (result < 0) {
            throw gcnew InvalidOperationException("Failed to write NAL unit to MP4 file");
        }
    }

#pragma endregion

#pragma region IDisposable

    void H26xWriter::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    H26xWriter::~H26xWriter() {
        this->!H26xWriter();
        GC::SuppressFinalize(this);
    }

    H26xWriter::!H26xWriter() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        // Close H26x writer and clean up internal state
        if (_writer != nullptr) {
            mp4_h26x_write_close(_writer);
            delete _writer;
            _writer = nullptr;
        }

        // Note: We don't delete _muxer as we don't own it
    }

#pragma endregion

}