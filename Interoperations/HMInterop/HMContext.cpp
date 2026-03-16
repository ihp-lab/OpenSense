#include "pch.h"

#pragma managed(push, off)
#include "TLibCommon/CommonDef.h"
#include "TLibCommon/TComRom.h"

static void NativeEnsureScanTables(int maxWidth, int maxHeight, int maxDepth) {
    // Scan tables need depth+1: initZscanToRaster(d) produces 4^(d-1) entries,
    // but CTU has 4^maxDepth partitions (unitSize = maxCUWidth >> maxDepth).
    auto scanDepth = maxDepth + 1;
    auto piTmp = &g_auiZscanToRaster[0];
    initZscanToRaster(scanDepth, 1, 0, piTmp);
    initRasterToZscan(maxWidth, maxHeight, scanDepth);
    initRasterToPelXY(maxWidth, maxHeight, scanDepth);
}

static void NativeInitRom() {
    initROM();
}

static void NativeDestroyRom() {
    destroyROM();
}
#pragma managed(pop)

#include "HMContext.h"

using namespace System;
using namespace System::Threading;

namespace HMInterop {

    void HMContext::Acquire(int maxWidth, int maxHeight, int maxDepth) {
        Monitor::Enter(s_lock);
        if (s_currentMaxCUWidth == maxWidth && s_currentMaxCUHeight == maxHeight && s_currentMaxTotalCUDepth == maxDepth) {
            return;
        }
        NativeEnsureScanTables(maxWidth, maxHeight, maxDepth);
        s_currentMaxCUWidth = maxWidth;
        s_currentMaxCUHeight = maxHeight;
        s_currentMaxTotalCUDepth = maxDepth;
    }

    void HMContext::Release() {
        Monitor::Exit(s_lock);
    }

    void HMContext::PrepareForInitialization() {
        if (!s_romValid) {
            return;
        }
        NativeDestroyRom();
        s_romValid = false;
    }

    void HMContext::NotifyInitializationComplete() {
        s_romValid = true;
        s_instanceCount++;
    }

    void HMContext::NotifyDestructionComplete() {
        s_instanceCount--;
        if (s_instanceCount <= 0) {
            s_romValid = false;
            return;
        }
        NativeInitRom();
        s_romValid = true;
    }
}
