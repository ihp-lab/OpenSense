#pragma once

#include "NvencHevcEnums.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::Serialization;

namespace FFMpegInterop {
    /// <summary>
    /// NVENC HEVC options. 
    /// </summary>
    /// <remarks>
    /// The full list of options can be found at <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/nvenc_hevc.c" />.
    /// The naming follows the naming in the options table.
    /// The order follows the order they appear in the options table.
    /// </remarks>
    public ref class NvencHevcOptions : public INotifyPropertyChanging, public INotifyPropertyChanged {
    private:
        int _preset;
        int _tune;
        int _profile;
        int _level;
        int _tier;
        int _device;
        int _rgbMode;

    public:
        NvencHevcOptions():
            _preset(static_cast<int>(NvencHevcPreset::P4)),
            _tune(static_cast<int>(NvencHevcTuningInfo::HighQuality)),
            _profile(static_cast<int>(NvencHevcProfile::Main)),
            _level(static_cast<int>(NvencHevcLevel::AutoSelect)),
            _tier(static_cast<int>(NvencHevcTier::Main)),
            _device(-1),
            _rgbMode(static_cast<int>(NvencHevcRgbMode::Yuv420))
            {}

        /// <summary> 
        /// Get or set the encoding preset.
        /// </summary>
        /// <remarks>
        /// Option "preset" in FFmpeg. Range from PRESET_DEFAULT to PRESET_P7. Default is PRESET_P4.
        /// </remarks>
        property int Preset {
            int get() { return _preset; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcPreset::Deafult) || value > static_cast<int>(NvencHevcPreset::P7)) {
                    throw gcnew ArgumentOutOfRangeException("Preset");
                }
                SetProperty(_preset, value, "Preset");
            }
        }

        [IgnoreDataMember]
        property NvencHevcPreset PresetEnum {
            NvencHevcPreset get() { return static_cast<NvencHevcPreset>(Preset); }
            void set(NvencHevcPreset value) { Preset = static_cast<int>(value); }
        }

        /// <summary>
        /// Get or set the encoding tuning info.
        /// </summary>
        /// <remarks>
        /// Option "tune" in FFmpeg. Range from NV_ENC_TUNING_INFO_HIGH_QUALITY to NV_ENC_TUNING_INFO_ULTRA_HIGH_QUALITY. Default is NV_ENC_TUNING_INFO_HIGH_QUALITY.
        /// </remarks>
        property int Tune {
            int get() { return _tune; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcTuningInfo::HighQuality) || value > static_cast<int>(NvencHevcTuningInfo::UltraHighQuality)) {
                    throw gcnew ArgumentOutOfRangeException("Tune");
                }
                SetProperty(_tune, value, "Tune");
            }
        }

        [IgnoreDataMember]
        property NvencHevcTuningInfo TuneEnum {
            NvencHevcTuningInfo get() { return static_cast<NvencHevcTuningInfo>(Tune); }
            void set(NvencHevcTuningInfo value) { Tune = static_cast<int>(value); }
        }

        /// <summary>
        /// Get or set the encoding profile.
        /// </summary>
        /// <remarks>
        /// Option "profile" in FFmpeg. Range from NV_ENC_HEVC_PROFILE_MAIN to NV_ENC_HEVC_PROFILE_MULTIVIEW_MAIN. Default is NV_ENC_HEVC_PROFILE_MAIN.
        /// </remarks>
        property int Profile {
            int get() { return _profile; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcProfile::Main) || value > static_cast<int>(NvencHevcProfile::MultiviewMain)) {
                    throw gcnew ArgumentOutOfRangeException("Profile");
                }
                SetProperty(_profile, value, "Profile");
            }
        }

        [IgnoreDataMember]
        property NvencHevcProfile ProfileEnum {
            NvencHevcProfile get() { return static_cast<NvencHevcProfile>(Profile); }
            void set(NvencHevcProfile value) { Profile = static_cast<int>(value); }
        }

        /// <summary>
        /// Get or set the encoding level restriction.
        /// </summary>
        /// <remarks>
        /// Option "level" in FFmpeg. Range from NV_ENC_LEVEL_AUTOSELECT to NV_ENC_LEVEL_HEVC_62. Default is NV_ENC_LEVEL_AUTOSELECT.
        /// </remarks>
        property int Level {
            int get() { return _level; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcLevel::AutoSelect) || value > static_cast<int>(NvencHevcLevel::L62)) {
                    throw gcnew ArgumentOutOfRangeException("Level");
                }
                SetProperty(_level, value, "Level");
            }
        }

        [IgnoreDataMember]
        property NvencHevcLevel LevelEnum {
            NvencHevcLevel get() { return static_cast<NvencHevcLevel>(Level); }
            void set(NvencHevcLevel value) { Level = static_cast<int>(value); }
        }

        /// <summary>
        /// Get or set the encoding tier.
        /// </summary>
        /// <remarks>
        /// Option "tier" in FFmpeg. Range from NV_ENC_TIER_HEVC_MAIN to NV_ENC_TIER_HEVC_HIGH. Default is NV_ENC_TIER_HEVC_MAIN.
        /// </remarks>
        property int Tier {
            int get() { return _tier; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcTier::Main) || value > static_cast<int>(NvencHevcTier::High)) {
                    throw gcnew ArgumentOutOfRangeException("Tier");
                }
                SetProperty(_tier, value, "Tier");
            }
        }

        [IgnoreDataMember]
        property NvencHevcTier TierEnum {
            NvencHevcTier get() { return static_cast<NvencHevcTier>(Tier); }
            void set(NvencHevcTier value) { Tier = static_cast<int>(value); }
        }

        /// <summary>
        /// Get or set the GPU device to use.
        /// </summary>
        /// <remarks>
        /// Option "gpu" in FFmpeg. Range from LIST_DEVICES (-2) to int.MaxValue. Default is ANY_DEVICE (-1).
        /// </remarks>
        property int Device {
            int get() { return _device; }
            void set(int value) {
                if (value < -2) {
                    throw gcnew ArgumentOutOfRangeException("Device");
                }
                SetProperty(_device, value, "Device");
            }
        }

        /// <summary>
        /// Configure how nvenc handles packed RGB input.
        /// </summary>
        /// <remarks>
        /// Option "rgb_mode" in FFmpeg. Range from NVENC_RGB_MODE_DISABLED (0) to int.MaxValue. Default is NVENC_RGB_MODE_420 (1).
        /// </remarks>
        property int RgbMode {  
            int get() { return _rgbMode; }
            void set(int value) {
                if (value < static_cast<int>(NvencHevcRgbMode::Disabled)) {
                    throw gcnew ArgumentOutOfRangeException("RgbMode");
                }
                SetProperty(_rgbMode, value, "RgbMode");
            }
        }

#pragma region INotifyPropertyChanging and INotifyPropertyChanged
    public:
        virtual event PropertyChangingEventHandler^ PropertyChanging;
        virtual event PropertyChangedEventHandler^ PropertyChanged;

    private:
        generic <typename T>
        void SetProperty(T% field, T value, [NotNull] System::String^ propertyName);
#pragma endregion
    };
}

