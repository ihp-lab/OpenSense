#pragma once

namespace HMInterop {
    /// <summary>
    /// Bit depths per channel type (maps to ::BitDepths in TypeDef.h).
    /// </summary>
    public value struct BitDepths {
        int Luma;
        int Chroma;
    };
}
