#pragma once

using namespace System;
using namespace System::Threading;

namespace HMInterop {
    /// <summary>
    /// Manages HM's process-wide global state (scan tables, ROM) for safe
    /// multi-instance usage. All encoder/decoder operations must go through
    /// this context to prevent data races and resource leaks.
    /// </summary>
    private ref class HMContext {
    private:
        static Object^ s_lock = gcnew Object();
        static int s_instanceCount = 0;
        static bool s_romValid = false;
        static int s_currentMaxCUWidth = 0;
        static int s_currentMaxCUHeight = 0;
        static int s_currentMaxTotalCUDepth = 0;

#pragma region Operation Scoping
    internal:
        /// <summary>
        /// Acquire the process-wide lock and ensure scan tables
        /// match the given CTU parameters. Call before any HM operation.
        /// Must be paired with Release().
        /// </summary>
        static void Acquire(int maxWidth, int maxHeight, int maxDepth);

        /// <summary>
        /// Release the process-wide lock.
        /// </summary>
        static void Release();
#pragma endregion

#pragma region Instance Lifecycle
    internal:
        /// <summary>
        /// Call inside Acquire/Release, BEFORE an HM function that
        /// internally calls initROM() (TDecTop::init, TEncTop::create).
        /// Frees existing ROM arrays to prevent allocation leak.
        /// </summary>
        static void PrepareForInitialization();

        /// <summary>
        /// Call inside Acquire/Release, AFTER an HM function that
        /// internally called initROM(). Marks ROM valid, increments
        /// instance count.
        /// </summary>
        static void NotifyInitializationComplete();

        /// <summary>
        /// Call inside Acquire/Release, AFTER an HM function that
        /// internally called destroyROM() (TDecTop::deletePicBuffer,
        /// TEncTop::destroy). Restores ROM for remaining instances
        /// if any, decrements instance count.
        /// </summary>
        static void NotifyDestructionComplete();
#pragma endregion
    };
}
