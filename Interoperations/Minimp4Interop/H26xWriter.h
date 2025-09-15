#pragma once

// Include minimp4 declarations only (not implementation)
extern "C" {
#include <minimp4.h>
}

#include "Enums.h"
#include "Muxer.h"

using namespace System;
using namespace System::IO;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace Minimp4Interop {
    /// <summary>
    /// Simplified H.264/H.265 video writer for MP4 files
    /// This class manages a single video track within an existing Muxer.
    /// It handles H.264/H.265 specific operations like NAL unit processing.
    /// Note: The caller retains ownership of the Muxer instance.
    /// </summary>
    public ref class H26xWriter : IDisposable {
    private:
        mp4_h26x_writer_t* _writer;
        Muxer^ _muxer;
        bool _isHEVC;
        int _width;
        int _height;
        bool _disposed;


    public:
        /// <summary>
        /// Creates a new H.264/H.265 video writer
        /// Note: The caller retains ownership of the Muxer. H26xWriter only uses it to write video data.
        /// </summary>
        /// <param name="muxer">The Muxer instance to write to (caller retains ownership)</param>
        /// <param name="width">Video width in pixels</param>
        /// <param name="height">Video height in pixels</param>
        /// <param name="isHEVC">True for HEVC/H.265, false for AVC/H.264</param>
        H26xWriter([NotNull] Muxer^ muxer, int width, int height, bool isHEVC);

#pragma region Methods

        /// <summary>
        /// Writes NAL unit data to the MP4 file
        /// </summary>
        /// <param name="nalData">NAL unit data pointer</param>
        /// <param name="size">NAL data size in bytes</param>
        /// <param name="timestamp90kHz">Presentation timestamp in 90kHz units (ignored for parameter sets)</param>
        void WriteNal(IntPtr nalData, int size, unsigned int timestamp90kHz);

#pragma endregion

#pragma region Properties

        /// <summary>
        /// Gets whether this writer is for HEVC
        /// </summary>
        property bool IsHEVC {
            bool get() {
                ThrowIfDisposed();
                return _isHEVC;
            }
        }

        /// <summary>
        /// Gets the video width
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                return _width;
            }
        }

        /// <summary>
        /// Gets the video height
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
                return _height;
            }
        }

#pragma endregion

#pragma region IDisposable
    private:
        /// <summary>
        /// Throws ObjectDisposedException if the object has been disposed
        /// </summary>
        void ThrowIfDisposed();

    public:
        ~H26xWriter();
        !H26xWriter();
#pragma endregion
    };
}