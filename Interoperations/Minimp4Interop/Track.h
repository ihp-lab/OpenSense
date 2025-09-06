#pragma once

extern "C" {
#include <minimp4.h>
}

#include "Enums.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics::CodeAnalysis;
using namespace System::Runtime::CompilerServices;

namespace Minimp4Interop {
    /// <summary>
    /// Managed wrapper for MP4 track configuration
    /// </summary>
    public ref class Track : IDisposable {
    private:
        MP4E_track_t* _track;
        bool _disposed;
        TrackType _trackType;

    private:
        Track();

    public:
        /// <summary>
        /// Creates a video track configuration
        /// </summary>
        static Track^ CreateVideoTrack(int width, int height, ObjectType objectType);

        /// <summary>
        /// Creates an audio track configuration
        /// </summary>
        static Track^ CreateAudioTrack(int sampleRate, int channels, ObjectType objectType);

#pragma region Properties

        /// <summary>
        /// Gets the track type (Video or Audio)
        /// </summary>
        property TrackType Type {
            TrackType get() {
                ThrowIfDisposed();
                return _trackType;
            }
        }

        /// <summary>
        /// Gets or sets the time scale (ticks per second)
        /// </summary>
        property int TimeScale {
            int get() {
                ThrowIfDisposed();
                return _track->time_scale;
            }
            void set(int value) {
                ThrowIfDisposed();
                if (value <= 0) {
                    throw gcnew ArgumentException("TimeScale must be positive");
                }
                _track->time_scale = value;
            }
        }

        /// <summary>
        /// Gets the width (for video tracks)
        /// </summary>
        property int Width {
            int get() {
                ThrowIfDisposed();
                if (_trackType != TrackType::Video) {
                    throw gcnew InvalidOperationException("Width is only available for video tracks");
                }
                return _track->u.v.width;
            }
        }

        /// <summary>
        /// Gets the height (for video tracks)
        /// </summary>
        property int Height {
            int get() {
                ThrowIfDisposed();
                if (_trackType != TrackType::Video) {
                    throw gcnew InvalidOperationException("Height is only available for video tracks");
                }
                return _track->u.v.height;
            }
        }

        /// <summary>
        /// Gets the sample rate (for audio tracks)
        /// </summary>
        property int SampleRate {
            int get() {
                ThrowIfDisposed();
                if (_trackType != TrackType::Audio) {
                    throw gcnew InvalidOperationException("SampleRate is only available for audio tracks");
                }
                return _track->time_scale;
            }
        }

        /// <summary>
        /// Gets the number of channels (for audio tracks)
        /// </summary>
        property int Channels {
            int get() {
                ThrowIfDisposed();
                if (_trackType != TrackType::Audio) {
                    throw gcnew InvalidOperationException("Channels is only available for audio tracks");
                }
                return _track->u.a.channelcount;
            }
        }

        /// <summary>
        /// Gets the object type (codec)
        /// </summary>
        property ObjectType ObjectTypeIndication {
            ObjectType get() {
                ThrowIfDisposed();
                return static_cast<ObjectType>(_track->object_type_indication);
            }
        }

#pragma endregion

#pragma region Internal

    internal:
        /// <summary>
        /// Gets the internal MP4E_track_t pointer
        /// </summary>
        property MP4E_track_t* InternalTrack {
            MP4E_track_t* get() {
                ThrowIfDisposed();
                return _track;
            }
        }

#pragma endregion

#pragma region IDisposable
    private:
        void ThrowIfDisposed();

    public:
        ~Track();
        !Track();
#pragma endregion
    };
}