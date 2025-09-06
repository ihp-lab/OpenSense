#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Enums.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_frame_info
    /// </summary>
    public ref class FrameInfo : IDisposable {
    private:
        kvz_frame_info* _info;
        bool _disposed;

    internal:
        /// <summary>
        /// Creates a FrameInfo from native pointer
        /// Takes ownership of the frame info
        /// </summary>
        FrameInfo(kvz_frame_info* info);

        /// <summary>
        /// Creates a FrameInfo by copying native struct
        /// </summary>
        FrameInfo(const kvz_frame_info& info);

    public:
        /// <summary>
        /// Gets the picture order count
        /// </summary>
        property int POC {
            int get() {
                ThrowIfDisposed();
                return _info->poc;
            }
        }

        /// <summary>
        /// Gets the quantization parameter
        /// </summary>
        property int QP {
            int get() {
                ThrowIfDisposed();
                return _info->qp;
            }
        }

        /// <summary>
        /// Gets the NAL unit type
        /// </summary>
        property NetworkAbstractionLayerUnitType NalUnitType {
            NetworkAbstractionLayerUnitType get() {
                ThrowIfDisposed();
                return static_cast<NetworkAbstractionLayerUnitType>(_info->nal_unit_type);
            }
        }

        /// <summary>
        /// Gets the slice type
        /// </summary>
        property SliceType SliceType {
            KvazaarInterop::SliceType get() {
                ThrowIfDisposed();
                return static_cast<KvazaarInterop::SliceType>(_info->slice_type);
            }
        }

        /// <summary>
        /// Gets the reference list for specified list index
        /// </summary>
        array<int>^ GetReferenceList(int listIndex);

        /// <summary>
        /// Gets the reference list length for specified list index
        /// </summary>
        int GetReferenceListLength(int listIndex);

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~FrameInfo();
        !FrameInfo();
#pragma endregion
    };
}