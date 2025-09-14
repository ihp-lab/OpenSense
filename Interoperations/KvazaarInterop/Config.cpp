#include "pch.h"

#include <msclr\marshal_cppstd.h>

#include "Config.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;

namespace KvazaarInterop {
    Config::Config()
        : _config(nullptr)
        , _disposed(false) {

        auto api = Api::GetApi();
        _config = api->config_alloc();

        if (!_config) {
            throw gcnew OutOfMemoryException("Failed to allocate config");
        }

        auto result = api->config_init(_config);
        if (!result) {
            api->config_destroy(_config);
            _config = nullptr;
            throw gcnew InvalidOperationException("Failed to initialize config");
        }
    }

    bool Config::Parse([NotNull] String^ name, [NotNull] String^ value) {
        ThrowIfDisposed();

        ArgumentNullException::ThrowIfNull(name, "name");
        ArgumentNullException::ThrowIfNull(value, "value");

        auto api = Api::GetApi();
        auto nativeName = marshal_as<std::string>(name);
        auto nativeValue = marshal_as<std::string>(value);

        auto result = api->config_parse(_config, nativeName.c_str(), nativeValue.c_str());
        return result != 0;
    }

#pragma region Properties

    int Config::QP::get() {
        ThrowIfDisposed();
        return _config->qp;
    }

    void Config::QP::set(int value) {
        ThrowIfDisposed();
        _config->qp = value;
    }

    int Config::IntraPeriod::get() {
        ThrowIfDisposed();
        return _config->intra_period;
    }

    void Config::IntraPeriod::set(int value) {
        ThrowIfDisposed();
        _config->intra_period = value;
    }

    int Config::VpsPeriod::get() {
        ThrowIfDisposed();
        return _config->vps_period;
    }

    void Config::VpsPeriod::set(int value) {
        ThrowIfDisposed();
        _config->vps_period = value;
    }

    int Config::Width::get() {
        ThrowIfDisposed();
        return _config->width;
    }

    void Config::Width::set(int value) {
        ThrowIfDisposed();
        if (value <= 0 || value % 8 != 0) {
            throw gcnew ArgumentException("Width must be positive and divisible by 8");
        }
        _config->width = value;
    }

    int Config::Height::get() {
        ThrowIfDisposed();
        return _config->height;
    }

    void Config::Height::set(int value) {
        ThrowIfDisposed();
        if (value <= 0 || value % 8 != 0) {
            throw gcnew ArgumentException("Height must be positive and divisible by 8");
        }
        _config->height = value;
    }

    int Config::FramerateNumerator::get() {
        ThrowIfDisposed();
        return _config->framerate_num;
    }

    void Config::FramerateNumerator::set(int value) {
        ThrowIfDisposed();
        _config->framerate_num = value;
    }

    int Config::FramerateDenominator::get() {
        ThrowIfDisposed();
        return _config->framerate_denom;
    }

    void Config::FramerateDenominator::set(int value) {
        ThrowIfDisposed();
        _config->framerate_denom = value;
    }

    bool Config::DeblockEnable::get() {
        ThrowIfDisposed();
        return _config->deblock_enable != 0;
    }

    void Config::DeblockEnable::set(bool value) {
        ThrowIfDisposed();
        _config->deblock_enable = value ? 1 : 0;
    }

    SampleAdaptiveOffset Config::SaoType::get() {
        ThrowIfDisposed();
        return static_cast<SampleAdaptiveOffset>(_config->sao_type);
    }

    void Config::SaoType::set(SampleAdaptiveOffset value) {
        ThrowIfDisposed();
        _config->sao_type = static_cast<kvz_sao>(value);
    }

    bool Config::RdoqEnable::get() {
        ThrowIfDisposed();
        return _config->rdoq_enable != 0;
    }

    void Config::RdoqEnable::set(bool value) {
        ThrowIfDisposed();
        _config->rdoq_enable = value ? 1 : 0;
    }

    bool Config::SignhideEnable::get() {
        ThrowIfDisposed();
        return _config->signhide_enable != 0;
    }

    void Config::SignhideEnable::set(bool value) {
        ThrowIfDisposed();
        _config->signhide_enable = value ? 1 : 0;
    }

    bool Config::SmpEnable::get() {
        ThrowIfDisposed();
        return _config->smp_enable != 0;
    }

    void Config::SmpEnable::set(bool value) {
        ThrowIfDisposed();
        _config->smp_enable = value ? 1 : 0;
    }

    bool Config::AmpEnable::get() {
        ThrowIfDisposed();
        return _config->amp_enable != 0;
    }

    void Config::AmpEnable::set(bool value) {
        ThrowIfDisposed();
        _config->amp_enable = value ? 1 : 0;
    }

    int Config::Rdo::get() {
        ThrowIfDisposed();
        return _config->rdo;
    }

    void Config::Rdo::set(int value) {
        ThrowIfDisposed();
        _config->rdo = value;
    }

    bool Config::FullIntraSearch::get() {
        ThrowIfDisposed();
        return _config->full_intra_search != 0;
    }

    void Config::FullIntraSearch::set(bool value) {
        ThrowIfDisposed();
        _config->full_intra_search = value ? 1 : 0;
    }

    bool Config::TrskipEnable::get() {
        ThrowIfDisposed();
        return _config->trskip_enable != 0;
    }

    void Config::TrskipEnable::set(bool value) {
        ThrowIfDisposed();
        _config->trskip_enable = value ? 1 : 0;
    }

    int Config::TrDepthIntra::get() {
        ThrowIfDisposed();
        return _config->tr_depth_intra;
    }

    void Config::TrDepthIntra::set(int value) {
        ThrowIfDisposed();
        _config->tr_depth_intra = value;
    }

    IntegerMotionEstimationAlgorithm Config::ImeAlgorithm::get() {
        ThrowIfDisposed();
        return static_cast<IntegerMotionEstimationAlgorithm>(_config->ime_algorithm);
    }

    void Config::ImeAlgorithm::set(IntegerMotionEstimationAlgorithm value) {
        ThrowIfDisposed();
        _config->ime_algorithm = static_cast<kvz_ime_algorithm>(value);
    }

    int Config::FmeLevel::get() {
        ThrowIfDisposed();
        return _config->fme_level;
    }

    void Config::FmeLevel::set(int value) {
        ThrowIfDisposed();
        _config->fme_level = value;
    }

    int Config::SourceScanType::get() {
        ThrowIfDisposed();
        return _config->source_scan_type;
    }

    void Config::SourceScanType::set(int value) {
        ThrowIfDisposed();
        _config->source_scan_type = value;
    }

    bool Config::Bipred::get() {
        ThrowIfDisposed();
        return _config->bipred != 0;
    }

    void Config::Bipred::set(bool value) {
        ThrowIfDisposed();
        _config->bipred = value ? 1 : 0;
    }

    int Config::DeblockBeta::get() {
        ThrowIfDisposed();
        return _config->deblock_beta;
    }

    void Config::DeblockBeta::set(int value) {
        ThrowIfDisposed();
        _config->deblock_beta = value;
    }

    int Config::DeblockTc::get() {
        ThrowIfDisposed();
        return _config->deblock_tc;
    }

    void Config::DeblockTc::set(int value) {
        ThrowIfDisposed();
        _config->deblock_tc = value;
    }

    bool Config::AudEnable::get() {
        ThrowIfDisposed();
        return _config->aud_enable != 0;
    }

    void Config::AudEnable::set(bool value) {
        ThrowIfDisposed();
        _config->aud_enable = value ? 1 : 0;
    }

    int Config::RefFrames::get() {
        ThrowIfDisposed();
        return _config->ref_frames;
    }

    void Config::RefFrames::set(int value) {
        ThrowIfDisposed();
        _config->ref_frames = value;
    }

    bool Config::Wpp::get() {
        ThrowIfDisposed();
        return _config->wpp != 0;
    }

    void Config::Wpp::set(bool value) {
        ThrowIfDisposed();
        _config->wpp = value ? 1 : 0;
    }

    int Config::Owf::get() {
        ThrowIfDisposed();
        return _config->owf;
    }

    void Config::Owf::set(int value) {
        ThrowIfDisposed();
        _config->owf = value;
    }

    int Config::Threads::get() {
        ThrowIfDisposed();
        return _config->threads;
    }

    void Config::Threads::set(int value) {
        ThrowIfDisposed();
        _config->threads = value;
    }

    bool Config::Cpuid::get() {
        ThrowIfDisposed();
        return _config->cpuid != 0;
    }

    void Config::Cpuid::set(bool value) {
        ThrowIfDisposed();
        _config->cpuid = value ? 1 : 0;
    }

    bool Config::AddEncoderInfo::get() {
        ThrowIfDisposed();
        return _config->add_encoder_info != 0;
    }

    void Config::AddEncoderInfo::set(bool value) {
        ThrowIfDisposed();
        _config->add_encoder_info = value ? 1 : 0;
    }

    int Config::GopLen::get() {
        ThrowIfDisposed();
        return _config->gop_len;
    }

    void Config::GopLen::set(int value) {
        ThrowIfDisposed();
        _config->gop_len = value;
    }

    bool Config::GopLowDelay::get() {
        ThrowIfDisposed();
        return _config->gop_lowdelay != 0;
    }

    void Config::GopLowDelay::set(bool value) {
        ThrowIfDisposed();
        _config->gop_lowdelay = value ? 1 : 0;
    }

    int Config::TargetBitrate::get() {
        ThrowIfDisposed();
        return _config->target_bitrate;
    }

    void Config::TargetBitrate::set(int value) {
        ThrowIfDisposed();
        _config->target_bitrate = value;
    }

    bool Config::MvRdo::get() {
        ThrowIfDisposed();
        return _config->mv_rdo != 0;
    }

    void Config::MvRdo::set(bool value) {
        ThrowIfDisposed();
        _config->mv_rdo = value ? 1 : 0;
    }

    bool Config::CalcPsnr::get() {
        ThrowIfDisposed();
        return _config->calc_psnr != 0;
    }

    void Config::CalcPsnr::set(bool value) {
        ThrowIfDisposed();
        _config->calc_psnr = value ? 1 : 0;
    }

    MotionVectorConstraint Config::MvConstraint::get() {
        ThrowIfDisposed();
        return static_cast<MotionVectorConstraint>(_config->mv_constraint);
    }

    void Config::MvConstraint::set(MotionVectorConstraint value) {
        ThrowIfDisposed();
        _config->mv_constraint = static_cast<kvz_mv_constraint>(value);
    }

    HashAlgorithm Config::Hash::get() {
        ThrowIfDisposed();
        return static_cast<HashAlgorithm>(_config->hash);
    }

    void Config::Hash::set(HashAlgorithm value) {
        ThrowIfDisposed();
        _config->hash = static_cast<kvz_hash>(value);
    }

    MotionEstimationEarlyTermination Config::MeEarlyTermination::get() {
        ThrowIfDisposed();
        return static_cast<MotionEstimationEarlyTermination>(_config->me_early_termination);
    }

    void Config::MeEarlyTermination::set(MotionEstimationEarlyTermination value) {
        ThrowIfDisposed();
        _config->me_early_termination = static_cast<kvz_me_early_termination>(value);
    }

    bool Config::IntraRdoEt::get() {
        ThrowIfDisposed();
        return _config->intra_rdo_et != 0;
    }

    void Config::IntraRdoEt::set(bool value) {
        ThrowIfDisposed();
        _config->intra_rdo_et = value ? 1 : 0;
    }

    bool Config::Lossless::get() {
        ThrowIfDisposed();
        return _config->lossless != 0;
    }

    void Config::Lossless::set(bool value) {
        ThrowIfDisposed();
        _config->lossless = value ? 1 : 0;
    }

    bool Config::TmvpEnable::get() {
        ThrowIfDisposed();
        return _config->tmvp_enable != 0;
    }

    void Config::TmvpEnable::set(bool value) {
        ThrowIfDisposed();
        _config->tmvp_enable = value ? 1 : 0;
    }

    int Config::RdoqSkip::get() {
        ThrowIfDisposed();
        return _config->rdoq_skip;
    }

    void Config::RdoqSkip::set(int value) {
        ThrowIfDisposed();
        _config->rdoq_skip = value;
    }

    InputFormat Config::InputFormat::get() {
        ThrowIfDisposed();
        return static_cast<KvazaarInterop::InputFormat>(_config->input_format);
    }

    void Config::InputFormat::set(KvazaarInterop::InputFormat value) {
        ThrowIfDisposed();
        _config->input_format = static_cast<kvz_input_format>(value);
    }

    int Config::InputBitDepth::get() {
        ThrowIfDisposed();
        return _config->input_bitdepth;
    }

    void Config::InputBitDepth::set(int value) {
        ThrowIfDisposed();
        _config->input_bitdepth = value;
    }

    bool Config::ImplicitRdpcm::get() {
        ThrowIfDisposed();
        return _config->implicit_rdpcm != 0;
    }

    void Config::ImplicitRdpcm::set(bool value) {
        ThrowIfDisposed();
        _config->implicit_rdpcm = value ? 1 : 0;
    }

    bool Config::ErpAqp::get() {
        ThrowIfDisposed();
        return _config->erp_aqp != 0;
    }

    void Config::ErpAqp::set(bool value) {
        ThrowIfDisposed();
        _config->erp_aqp = value ? 1 : 0;
    }

    int Config::Level::get() {
        ThrowIfDisposed();
        return _config->level;
    }

    void Config::Level::set(int value) {
        ThrowIfDisposed();
        _config->level = value;
    }

    bool Config::ForceLevel::get() {
        ThrowIfDisposed();
        return _config->force_level != 0;
    }

    void Config::ForceLevel::set(bool value) {
        ThrowIfDisposed();
        _config->force_level = value ? 1 : 0;
    }

    bool Config::HighTier::get() {
        ThrowIfDisposed();
        return _config->high_tier != 0;
    }

    void Config::HighTier::set(bool value) {
        ThrowIfDisposed();
        _config->high_tier = value ? 1 : 0;
    }

    unsigned int Config::MeMaxSteps::get() {
        ThrowIfDisposed();
        return _config->me_max_steps;
    }

    void Config::MeMaxSteps::set(unsigned int value) {
        ThrowIfDisposed();
        _config->me_max_steps = value;
    }

    int Config::IntraQpOffset::get() {
        ThrowIfDisposed();
        return _config->intra_qp_offset;
    }

    void Config::IntraQpOffset::set(int value) {
        ThrowIfDisposed();
        _config->intra_qp_offset = value;
    }

    bool Config::IntraQpOffsetAuto::get() {
        ThrowIfDisposed();
        return _config->intra_qp_offset_auto != 0;
    }

    void Config::IntraQpOffsetAuto::set(bool value) {
        ThrowIfDisposed();
        _config->intra_qp_offset_auto = value ? 1 : 0;
    }

    int Config::FastResidualCostLimit::get() {
        ThrowIfDisposed();
        return _config->fast_residual_cost_limit;
    }

    void Config::FastResidualCostLimit::set(int value) {
        ThrowIfDisposed();
        _config->fast_residual_cost_limit = value;
    }

    bool Config::SetQpInCu::get() {
        ThrowIfDisposed();
        return _config->set_qp_in_cu != 0;
    }

    void Config::SetQpInCu::set(bool value) {
        ThrowIfDisposed();
        _config->set_qp_in_cu = value ? 1 : 0;
    }

    bool Config::OpenGop::get() {
        ThrowIfDisposed();
        return _config->open_gop != 0;
    }

    void Config::OpenGop::set(bool value) {
        ThrowIfDisposed();
        _config->open_gop = value ? 1 : 0;
    }

    int Config::Vaq::get() {
        ThrowIfDisposed();
        return _config->vaq;
    }

    void Config::Vaq::set(int value) {
        ThrowIfDisposed();
        _config->vaq = value;
    }

    KvazaarInterop::ScalingList Config::ScalingList::get() {
        ThrowIfDisposed();
        return static_cast<KvazaarInterop::ScalingList>(_config->scaling_list);
    }

    void Config::ScalingList::set(KvazaarInterop::ScalingList value) {
        ThrowIfDisposed();
        _config->scaling_list = static_cast<int8_t>(value);
    }

    int Config::MaxMerge::get() {
        ThrowIfDisposed();
        return _config->max_merge;
    }

    void Config::MaxMerge::set(int value) {
        ThrowIfDisposed();
        _config->max_merge = value;
    }

    bool Config::EarlySkip::get() {
        ThrowIfDisposed();
        return _config->early_skip != 0;
    }

    void Config::EarlySkip::set(bool value) {
        ThrowIfDisposed();
        _config->early_skip = value ? 1 : 0;
    }

    bool Config::MlPuDepthIntra::get() {
        ThrowIfDisposed();
        return _config->ml_pu_depth_intra != 0;
    }

    void Config::MlPuDepthIntra::set(bool value) {
        ThrowIfDisposed();
        _config->ml_pu_depth_intra = value ? 1 : 0;
    }

    bool Config::ZeroCoeffRdo::get() {
        ThrowIfDisposed();
        return _config->zero_coeff_rdo != 0;
    }

    void Config::ZeroCoeffRdo::set(bool value) {
        ThrowIfDisposed();
        _config->zero_coeff_rdo = value ? 1 : 0;
    }

    RateControlAlgorithm Config::RcAlgorithm::get() {
        ThrowIfDisposed();
        return static_cast<RateControlAlgorithm>(_config->rc_algorithm);
    }

    void Config::RcAlgorithm::set(RateControlAlgorithm value) {
        ThrowIfDisposed();
        _config->rc_algorithm = static_cast<int8_t>(value);
    }

    bool Config::IntraBitAllocation::get() {
        ThrowIfDisposed();
        return _config->intra_bit_allocation != 0;
    }

    void Config::IntraBitAllocation::set(bool value) {
        ThrowIfDisposed();
        _config->intra_bit_allocation = value ? 1 : 0;
    }

    bool Config::ClipNeighbour::get() {
        ThrowIfDisposed();
        return _config->clip_neighbour != 0;
    }

    void Config::ClipNeighbour::set(bool value) {
        ThrowIfDisposed();
        _config->clip_neighbour = value ? 1 : 0;
    }

    KvazaarInterop::FileFormat Config::FileFormat::get() {
        ThrowIfDisposed();
        return static_cast<KvazaarInterop::FileFormat>(_config->file_format);
    }

    void Config::FileFormat::set(KvazaarInterop::FileFormat value) {
        ThrowIfDisposed();
        _config->file_format = static_cast<kvz_file_format>(value);
    }

    bool Config::CombineIntraCus::get() {
        ThrowIfDisposed();
        return _config->combine_intra_cus != 0;
    }

    void Config::CombineIntraCus::set(bool value) {
        ThrowIfDisposed();
        _config->combine_intra_cus = value ? 1 : 0;
    }

    bool Config::ForceInter::get() {
        ThrowIfDisposed();
        return _config->force_inter != 0;
    }

    void Config::ForceInter::set(bool value) {
        ThrowIfDisposed();
        _config->force_inter = value ? 1 : 0;
    }

    bool Config::IntraChromaSearch::get() {
        ThrowIfDisposed();
        return _config->intra_chroma_search != 0;
    }

    void Config::IntraChromaSearch::set(bool value) {
        ThrowIfDisposed();
        _config->intra_chroma_search = value ? 1 : 0;
    }

    bool Config::FastBipred::get() {
        ThrowIfDisposed();
        return _config->fast_bipred != 0;
    }

    void Config::FastBipred::set(bool value) {
        ThrowIfDisposed();
        _config->fast_bipred = value ? 1 : 0;
    }

    bool Config::EnableLoggingOutput::get() {
        ThrowIfDisposed();
        return _config->enable_logging_output != 0;
    }

    void Config::EnableLoggingOutput::set(bool value) {
        ThrowIfDisposed();
        _config->enable_logging_output = value ? 1 : 0;
    }

    kvz_config* Config::InternalConfig::get() {
        ThrowIfDisposed();
        return _config;
    }

#pragma endregion

    void Config::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Config::~Config() {
        if (_disposed) {
            return;
        }

        this->!Config();
        _disposed = true;
    }

    Config::!Config() {
        if (_config) {
            auto api = Api::GetApi();
            api->config_destroy(_config);
            _config = nullptr;
        }
    }
}