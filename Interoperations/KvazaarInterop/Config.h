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
        /// Quantization parameter.
        /// Default value is 22.
        /// </summary>
        property int QP {
            int get();
            void set(int value);
        }

        /// <summary>
        /// The period of intra frames in stream.
        /// 0: only first picture is intra.
        /// 1: all pictures are intra.
        /// N: every Nth picture is intra.
        /// Default value is 64.
        /// </summary>
        property int IntraPeriod {
            int get();
            void set(int value);
        }

        /// <summary>
        /// How often the VPS, SPS and PPS are re-sent.
        /// -1: never.
        ///  0: first frame only.
        ///  1: every intra frame.
        ///  2: every other intra frame.
        ///  3: every third intra frame.
        /// Default value is 0.
        /// </summary>
        property int VpsPeriod {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Frame width, must be a multiple of 8.
        /// Default value is 0.
        /// </summary>
        property int Width {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Frame height, must be a multiple of 8.
        /// Default value is 0.
        /// </summary>
        property int Height {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Framerate numerator.
        /// Default value is 25.
        /// </summary>
        property int FramerateNumerator {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Framerate denominator.
        /// Default value is 1.
        /// </summary>
        property int FramerateDenominator {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Flag to enable deblocking filter.
        /// Default value is true.
        /// </summary>
        property bool DeblockEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable sample adaptive offset filter.
        /// Default value is 3.
        /// </summary>
        property SampleAdaptiveOffset SaoType {
            SampleAdaptiveOffset get();
            void set(SampleAdaptiveOffset value);
        }

        /// <summary>
        /// Flag to enable RD optimized quantization.
        /// Default value is true.
        /// </summary>
        property bool RdoqEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable sign hiding.
        /// Default value is true.
        /// </summary>
        property bool SignhideEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable SMP blocks.
        /// Default value is false.
        /// </summary>
        property bool SmpEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable AMP blocks.
        /// Default value is false.
        /// </summary>
        property bool AmpEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// RD-calculation level.
        /// Range is 0 to 2.
        /// Default value is 1.
        /// </summary>
        property int Rdo {
            int get();
            void set(int value);
        }

        /// <summary>
        /// If true, don't skip modes in intra search.
        /// Default value is false.
        /// </summary>
        property bool FullIntraSearch {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable transform skip for 4x4 blocks.
        /// Default value is false.
        /// </summary>
        property bool TrskipEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Maximum transform depth for intra.
        /// Default value is 0.
        /// </summary>
        property int TrDepthIntra {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Integer motion estimation algorithm.
        /// Default value is KVZ_IME_HEXBS.
        /// </summary>
        property IntegerMotionEstimationAlgorithm ImeAlgorithm {
            IntegerMotionEstimationAlgorithm get();
            void set(IntegerMotionEstimationAlgorithm value);
        }

        /// <summary>
        /// Fractional pixel motion estimation level.
        /// 0: disabled, 1-4: enabled with different accuracies.
        /// Default value is 4.
        /// </summary>
        property int FmeLevel {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Source scan type.
        /// 0: progressive, 1: top field first, 2: bottom field first.
        /// Default value is 0.
        /// </summary>
        property int SourceScanType {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Bi-prediction.
        /// Default value is false.
        /// </summary>
        property bool Bipred {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Deblocking beta offset (div 2).
        /// Range is -6 to 6.
        /// Default value is 0.
        /// </summary>
        property int DeblockBeta {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Deblocking tc offset (div 2).
        /// Range is -6 to 6.
        /// Default value is 0.
        /// </summary>
        property int DeblockTc {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Flag to use access unit delimiters.
        /// Default value is false.
        /// </summary>
        property bool AudEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Number of reference frames to use.
        /// Default value is 1.
        /// </summary>
        property int RefFrames {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Wavefront parallel processing.
        /// Default value is true.
        /// </summary>
        property bool Wpp {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Frame-level parallelism.
        /// Set to 0 to disable, positive values indicate number of frames.
        /// Default value is -1 (auto).
        /// </summary>
        property int Owf {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Number of threads.
        /// Default value is -1 (auto).
        /// </summary>
        property int Threads {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Enable CPU optimizations.
        /// Default value is true.
        /// </summary>
        property bool Cpuid {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Add encoder information SEI message.
        /// Default value is true.
        /// </summary>
        property bool AddEncoderInfo {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Length of GOP for the video sequence.
        /// Default value is 4.
        /// </summary>
        property int GopLen {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Specifies that the GOP does not use future pictures.
        /// Default value is true.
        /// </summary>
        property bool GopLowDelay {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Target bitrate in bits per second.
        /// Default value is 0 (disabled).
        /// </summary>
        property int TargetBitrate {
            int get();
            void set(int value);
        }

        /// <summary>
        /// MV RDO calculation in search.
        /// 0: estimation, 1: RDO.
        /// Default value is false.
        /// </summary>
        property bool MvRdo {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Calculate PSNR.
        /// Default value is true.
        /// </summary>
        property bool CalcPsnr {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Constrain movement vectors.
        /// Default value is KVZ_MV_CONSTRAIN_NONE.
        /// </summary>
        property MotionVectorConstraint MvConstraint {
            MotionVectorConstraint get();
            void set(MotionVectorConstraint value);
        }

        /// <summary>
        /// What hash algorithm to use.
        /// Default value is KVZ_HASH_CHECKSUM.
        /// </summary>
        property HashAlgorithm Hash {
            HashAlgorithm get();
            void set(HashAlgorithm value);
        }

        /// <summary>
        /// Motion estimation early termination.
        /// Default value is KVZ_ME_EARLY_TERMINATION_ON.
        /// </summary>
        property MotionEstimationEarlyTermination MeEarlyTermination {
            MotionEstimationEarlyTermination get();
            void set(MotionEstimationEarlyTermination value);
        }

        /// <summary>
        /// Use early termination in intra RDO.
        /// Default value is false.
        /// </summary>
        property bool IntraRdoEt {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Use lossless coding.
        /// Default value is false.
        /// </summary>
        property bool Lossless {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Use Temporal Motion Vector Predictors.
        /// Default value is true.
        /// </summary>
        property bool TmvpEnable {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Mode of RDOQ skip.
        /// Default value is 1.
        /// </summary>
        property int RdoqSkip {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Input picture format.
        /// Default value is KVZ_FORMAT_P420.
        /// </summary>
        property InputFormat InputFormat {
            KvazaarInterop::InputFormat get();
            void set(KvazaarInterop::InputFormat value);
        }

        /// <summary>
        /// Input bit depth.
        /// Default value is 8.
        /// </summary>
        property int InputBitDepth {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Enable implicit residual DPCM.
        /// Default value is false.
        /// </summary>
        property bool ImplicitRdpcm {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Use adaptive QP for 360 video with equirectangular projection.
        /// Default value is false.
        /// </summary>
        property bool ErpAqp {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// The HEVC level.
        /// Default value is 62 (level 6.2, the highest).
        /// </summary>
        property int Level {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Whether to ignore level limit violations.
        /// Default value is true.
        /// </summary>
        property bool ForceLevel {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Whether to use high tier bitrates.
        /// Requires the level to be 4.0 or higher.
        /// Default value is false.
        /// </summary>
        property bool HighTier {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Maximum steps that hexagonal and diagonal motion estimation can use.
        /// -1 to disable limit.
        /// Default value is -1.
        /// </summary>
        property unsigned int MeMaxSteps {
            unsigned int get();
            void set(unsigned int value);
        }

        /// <summary>
        /// Offset to add to QP for intra frames.
        /// Default value is 0.
        /// </summary>
        property int IntraQpOffset {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Select intra QP offset automatically based on GOP length.
        /// Default value is true.
        /// </summary>
        property bool IntraQpOffsetAuto {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Minimum QP that uses CABAC for residual cost instead of a fast estimate.
        /// Default value is 0.
        /// </summary>
        property int FastResidualCostLimit {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Set QP at CU level keeping pic_init_qp_minus26 in PPS zero.
        /// Default value is false.
        /// </summary>
        property bool SetQpInCu {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Flag to enable open GOP configuration.
        /// Default value is true.
        /// </summary>
        property bool OpenGop {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Enable variance adaptive quantization.
        /// Default value is 0.
        /// </summary>
        property int Vaq {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Type of scaling lists to use.
        /// Default value is KVZ_SCALING_LIST_OFF.
        /// </summary>
        property KvazaarInterop::ScalingList ScalingList {
            KvazaarInterop::ScalingList get();
            void set(KvazaarInterop::ScalingList value);
        }

        /// <summary>
        /// Maximum number of merge candidates.
        /// Default value is 5.
        /// </summary>
        property int MaxMerge {
            int get();
            void set(int value);
        }

        /// <summary>
        /// Enable Early Skip Mode Decision.
        /// Default value is true.
        /// </summary>
        property bool EarlySkip {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Enable Machine learning CU depth prediction for Intra encoding.
        /// Default value is false.
        /// </summary>
        property bool MlPuDepthIntra {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Always consider CU without any quantized residual.
        /// Default value is true.
        /// </summary>
        property bool ZeroCoeffRdo {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Rate control algorithm to use.
        /// Default value is KVZ_NO_RC.
        /// </summary>
        property RateControlAlgorithm RcAlgorithm {
            RateControlAlgorithm get();
            void set(RateControlAlgorithm value);
        }

        /// <summary>
        /// Whether to use hadamard based bit allocation for intra frames.
        /// Default value is false.
        /// </summary>
        property bool IntraBitAllocation {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Clip intra predictions to neighbour pixels.
        /// Default value is true.
        /// </summary>
        property bool ClipNeighbour {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Input file format.
        /// Default value is KVZ_FORMAT_AUTO.
        /// </summary>
        property KvazaarInterop::FileFormat FileFormat {
            KvazaarInterop::FileFormat get();
            void set(KvazaarInterop::FileFormat value);
        }

        /// <summary>
        /// Try combining intra CUs at lower depth.
        /// Default value is true.
        /// </summary>
        property bool CombineIntraCus {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Force inter mode for all CUs.
        /// Default value is false.
        /// </summary>
        property bool ForceInter {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Enable chroma search in intra mode.
        /// Default value is false.
        /// </summary>
        property bool IntraChromaSearch {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Enable fast bipred mode.
        /// Default value is true.
        /// </summary>
        property bool FastBipred {
            bool get();
            void set(bool value);
        }

        /// <summary>
        /// Enable logging output to stderr.
        /// Default value is true.
        /// </summary>
        property bool EnableLoggingOutput {
            bool get();
            void set(bool value);
        }

    internal:
        /// <summary>
        /// Gets the internal kvz_config pointer
        /// </summary>
        property kvz_config* InternalConfig {
            kvz_config* get();
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