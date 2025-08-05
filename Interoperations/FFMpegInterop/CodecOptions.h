#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Diagnostics::CodeAnalysis;

namespace FFMpegInterop {
    /// <summary>
    /// Common codec options. 
    /// </summary>
    /// <remarks>
    /// The full list of options can be found at <see href="https://github.com/FFmpeg/FFmpeg/blob/master/libavcodec/options_table.h" />.
    /// The naming follows the naming in the options table.
    /// The order follows the order they appear in the options table.
    /// </remarks>
    public ref class CodecOptions : public INotifyPropertyChanging, public INotifyPropertyChanged {
    private:
        int _gopSize;
        int _maxBFrames;

    public:
        CodecOptions() : 
            _gopSize(12), 
            _maxBFrames(0) 
            {}

        /// <summary>
        /// Get or set the group of picture (GOP) size.
        /// </summary>
        /// <remarks>
        /// Option "g" in FFmpeg. Range from int.MinValue to int.MaxValue. Default is 12.
        /// </remarks>
        property int GopSize {
            int get() {
                return _gopSize;
            }
            void set(int value) {
                SetProperty(_gopSize, value, "GopSize");
            }
        }

        /// <summary>
        /// Get or set the maximum number of B-frames between non-B-frames.
        /// </summary>
        /// <remarks>
        /// Option "bf" in FFmpeg. Range from -1 to int.MaxValue. Default is 0.
        /// </remarks>
        property int MaxBFrames {
            int get() {
                return _maxBFrames;
            }
            void set(int value) {
                if (value < -1) {
                    throw gcnew ArgumentOutOfRangeException("MaxBFrames", "MaxBFrames must be greater than or equal to -1");
                }
                SetProperty(_maxBFrames, value, "MaxBFrames");
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

