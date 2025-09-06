#pragma once

extern "C" {
#include <kvazaar.h>
}

#include "Enums.h"
#include "Api.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace KvazaarInterop {
    /// <summary>
    /// Managed wrapper for kvz_config
    /// </summary>
    public ref class Config : IDisposable {
    private:
        kvz_config* _config;
        bool _disposed;

    public:
        /// <summary>
        /// Creates and initializes a new configuration
        /// </summary>
        Config();

        /// <summary>
        /// Parse configuration from string parameters
        /// </summary>
        bool Parse([NotNull] String^ name, [NotNull] String^ value);

        /// <summary>
        /// Gets or sets the quantization parameter
        /// </summary>
        property int QP {
            int get() {
                ThrowIfDisposed();
                return _config->qp;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->qp = value;
            }
        }

        /// <summary>
        /// Gets or sets the width
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                return _config->width;
            }
            void set(int value) {
                ThrowIfDisposed();
                if (value <= 0 || value % 8 != 0) {
                    throw gcnew ArgumentException("Width must be positive and divisible by 8");
                }
                _config->width = value;
            }
        }

        /// <summary>
        /// Gets or sets the height
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
                return _config->height;
            }
            void set(int value) {
                ThrowIfDisposed();
                if (value <= 0 || value % 8 != 0) {
                    throw gcnew ArgumentException("Height must be positive and divisible by 8");
                }
                _config->height = value;
            }
        }

        /// <summary>
        /// Gets or sets the intra period
        /// </summary>
        property int IntraPeriod {
            int get() {
                ThrowIfDisposed();
                return _config->intra_period;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->intra_period = value;
            }
        }

        /// <summary>
        /// Gets or sets the framerate numerator
        /// </summary>
        property int FramerateNumerator {
            int get() {
                ThrowIfDisposed();
                return _config->framerate_num;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->framerate_num = value;
            }
        }

        /// <summary>
        /// Gets or sets the framerate denominator
        /// </summary>
        property int FramerateDenominator {
            int get() {
                ThrowIfDisposed();
                return _config->framerate_denom;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->framerate_denom = value;
            }
        }

        /// <summary>
        /// Gets or sets the target bitrate
        /// </summary>
        property int TargetBitrate {
            int get() {
                ThrowIfDisposed();
                return _config->target_bitrate;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->target_bitrate = value;
            }
        }

        /// <summary>
        /// Gets or sets the VPS period
        /// </summary>
        property int VpsPeriod {
            int get() {
                ThrowIfDisposed();
                return _config->vps_period;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->vps_period = value;
            }
        }

        /// <summary>
        /// Gets or sets whether deblocking is enabled
        /// </summary>
        property bool DeblockEnable {
            bool get() {
                ThrowIfDisposed();
                return _config->deblock_enable != 0;
            }
            void set(bool value) {
                ThrowIfDisposed();
                _config->deblock_enable = value ? 1 : 0;
            }
        }

        /// <summary>
        /// Gets or sets the SAO type
        /// </summary>
        property SampleAdaptiveOffset SaoType {
            SampleAdaptiveOffset get() {
                ThrowIfDisposed();
                return static_cast<SampleAdaptiveOffset>(_config->sao_type);
            }
            void set(SampleAdaptiveOffset value) {
                ThrowIfDisposed();
                _config->sao_type = static_cast<kvz_sao>(value);
            }
        }

        /// <summary>
        /// Gets or sets the IME algorithm
        /// </summary>
        property IntegerMotionEstimationAlgorithm ImeAlgorithm {
            IntegerMotionEstimationAlgorithm get() {
                ThrowIfDisposed();
                return static_cast<IntegerMotionEstimationAlgorithm>(_config->ime_algorithm);
            }
            void set(IntegerMotionEstimationAlgorithm value) {
                ThrowIfDisposed();
                _config->ime_algorithm = static_cast<kvz_ime_algorithm>(value);
            }
        }

        /// <summary>
        /// Gets or sets the motion vector constraint
        /// </summary>
        property MotionVectorConstraint MvConstraint {
            MotionVectorConstraint get() {
                ThrowIfDisposed();
                return static_cast<MotionVectorConstraint>(_config->mv_constraint);
            }
            void set(MotionVectorConstraint value) {
                ThrowIfDisposed();
                _config->mv_constraint = static_cast<kvz_mv_constraint>(value);
            }
        }

        /// <summary>
        /// Gets or sets the hash algorithm
        /// </summary>
        property HashAlgorithm Hash {
            HashAlgorithm get() {
                ThrowIfDisposed();
                return static_cast<HashAlgorithm>(_config->hash);
            }
            void set(HashAlgorithm value) {
                ThrowIfDisposed();
                _config->hash = static_cast<kvz_hash>(value);
            }
        }

        /// <summary>
        /// Gets or sets the input format
        /// </summary>
        property InputFormat InputFormat {
            KvazaarInterop::InputFormat get() {
                ThrowIfDisposed();
                return static_cast<KvazaarInterop::InputFormat>(_config->input_format);
            }
            void set(KvazaarInterop::InputFormat value) {
                ThrowIfDisposed();
                _config->input_format = static_cast<kvz_input_format>(value);
            }
        }

        /// <summary>
        /// Gets or sets the input bit depth
        /// </summary>
        property int InputBitDepth {
            int get() {
                ThrowIfDisposed();
                return _config->input_bitdepth;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->input_bitdepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of threads
        /// </summary>
        property int Threads {
            int get() {
                ThrowIfDisposed();
                return _config->threads;
            }
            void set(int value) {
                ThrowIfDisposed();
                _config->threads = value;
            }
        }

    internal:
        /// <summary>
        /// Gets the internal kvz_config pointer
        /// </summary>
        property kvz_config* InternalConfig {
            kvz_config* get() {
                ThrowIfDisposed();
                return _config;
            }
        }

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~Config();
        !Config();
#pragma endregion
    };
}