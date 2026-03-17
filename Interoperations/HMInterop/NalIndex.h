#pragma once

using namespace System;
using namespace System::Runtime::InteropServices;

namespace HMInterop {
    /// <summary>
    /// Index entry describing the location of a NAL unit within an AccessUnit buffer.
    /// Offset points past the Annex B start code; Length excludes the start code.
    /// </summary>
    [StructLayout(LayoutKind::Sequential)]
    public value struct NalIndex {
        /// <summary>Byte offset of the raw NAL data within the buffer (past the Annex B start code).</summary>
        initonly int Offset;
        /// <summary>Length in bytes of the raw NAL data (excluding Annex B start code).</summary>
        initonly int Length;

        NalIndex(int offset, int length) : Offset(offset), Length(length) {}
    };
}
