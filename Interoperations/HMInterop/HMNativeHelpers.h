#pragma once

#pragma managed(push, off)

#include "TLibCommon/TComPicYuv.h"

// Copy pixel data between TComPicYuv instances (stride-aware, row by row)
static inline void CopyPicYuvData(TComPicYuv* src, TComPicYuv* dst) {
    auto chromaFmt = src->getChromaFormat();
    auto numComponents = getNumberValidComponents(chromaFmt);
    for (UInt comp = 0; comp < numComponents; comp++) {
        auto compId = static_cast<ComponentID>(comp);
        auto srcAddr = src->getAddr(compId);
        auto dstAddr = dst->getAddr(compId);
        auto srcStride = src->getStride(compId);
        auto dstStride = dst->getStride(compId);
        auto compWidth = src->getWidth(compId);
        auto compHeight = src->getHeight(compId);

        for (int y = 0; y < compHeight; y++) {
            memcpy(dstAddr + y * dstStride, srcAddr + y * srcStride, compWidth * sizeof(Pel));
        }
    }
}

#pragma managed(pop)
