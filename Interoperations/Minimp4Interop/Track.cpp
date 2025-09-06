#include "pch.h"
#include "Track.h"

#include <cstring>

using namespace System;
using namespace System::Runtime::InteropServices;

namespace Minimp4Interop {

    Track::Track()
        : _track(nullptr)
        , _disposed(false)
        , _trackType(TrackType::Video) {
    }

    Track^ Track::CreateVideoTrack(int width, int height, ObjectType objectType) {
        if (width <= 0) {
            throw gcnew ArgumentException("Width must be positive", "width");
        }
        if (height <= 0) {
            throw gcnew ArgumentException("Height must be positive", "height");
        }

        Track^ track = gcnew Track();
        track->_trackType = TrackType::Video;
        track->_track = new MP4E_track_t();
        memset(track->_track, 0, sizeof(MP4E_track_t));

        track->_track->track_media_kind = e_video;
        track->_track->object_type_indication = static_cast<int>(objectType);
        track->_track->time_scale = 90000; // Default for video
        track->_track->default_duration = 0;
        track->_track->u.v.width = width;
        track->_track->u.v.height = height;

        return track;
    }

    Track^ Track::CreateAudioTrack(int sampleRate, int channels, ObjectType objectType) {
        if (sampleRate <= 0) {
            throw gcnew ArgumentException("Sample rate must be positive", "sampleRate");
        }
        if (channels <= 0) {
            throw gcnew ArgumentException("Channels must be positive", "channels");
        }

        Track^ track = gcnew Track();
        track->_trackType = TrackType::Audio;
        track->_track = new MP4E_track_t();
        memset(track->_track, 0, sizeof(MP4E_track_t));

        track->_track->track_media_kind = e_audio;
        track->_track->object_type_indication = static_cast<int>(objectType);
        track->_track->time_scale = sampleRate;
        track->_track->default_duration = 0;
        track->_track->u.a.channelcount = channels;

        return track;
    }

    void Track::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException(this->GetType()->FullName);
        }
    }

    Track::~Track() {
        if (_disposed) {
            return;
        }

        this->!Track();
        _disposed = true;
    }

    Track::!Track() {
        if (_track) {
            delete _track;
            _track = nullptr;
        }
    }

}