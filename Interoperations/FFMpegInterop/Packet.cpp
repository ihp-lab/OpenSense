#include "pch.h"

#include "Packet.h"

using namespace System;

namespace FFMpegInterop {

    Packet::Packet(AVPacket* packet, CodecParameters^ parameters)
        : _packet(packet)
        , _parameters(parameters)
        , _disposed(false) {
        if (packet == nullptr) {
            throw gcnew ArgumentNullException("packet");
        }
        if (parameters == nullptr) {
            throw gcnew ArgumentNullException("parameters");
        }
    }

    AVPacket* Packet::InternalPacket::get() {
        ThrowIfDisposed();
        return _packet;
    }

    CodecParameters^ Packet::Parameters::get() {
        ThrowIfDisposed();
        return _parameters;
    }

    long long Packet::PresentationTimeOffset::get() {
        ThrowIfDisposed();
        return _packet->pts;
    }

    long long Packet::DecodingTimeOffset::get() {
        ThrowIfDisposed();
        return _packet->dts;
    }

    long long Packet::Duration::get() {
        ThrowIfDisposed();
        return _packet->duration;
    }

    bool Packet::IsKeyFrame::get() {
        ThrowIfDisposed();
        return (_packet->flags & AV_PKT_FLAG_KEY) != 0;
    }

    int Packet::Size::get() {
        ThrowIfDisposed();
        return _packet->size;
    }

#pragma region IDisposable
    void Packet::ThrowIfDisposed() {
        if (_disposed) {
            throw gcnew ObjectDisposedException("Packet");
        }
    }

    Packet::~Packet() {
        this->!Packet();
        GC::SuppressFinalize(this);
    }

    Packet::!Packet() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_packet) {
            auto packet = _packet;
            av_packet_free(&packet);
            _packet = nullptr;
        }
    }
#pragma endregion
}
