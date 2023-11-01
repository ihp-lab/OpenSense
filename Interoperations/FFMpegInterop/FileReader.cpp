#include "pch.h"
#include "FileReader.h"

#include <string>

namespace FFMpegInterop {

    FileReader::FileReader(String^ filename, Action<int>^ callback) : _unmanaged(new FileReaderUnmanaged()), _callback(callback) {

        // Convert System::String to std::string
        std::string fname = msclr::interop::marshal_as<std::string>(filename);

        // Open video file
        if (avformat_open_input(&_unmanaged->formatContext, fname.c_str(), nullptr, nullptr) != 0) {
            throw gcnew System::Exception("Could not open file.");
        }

        // Retrieve stream information
        if (avformat_find_stream_info(_unmanaged->formatContext, nullptr) < 0) {
            throw gcnew System::Exception("Could not find stream information.");
        }

        // Find the first video stream
        for (unsigned i = 0; i < _unmanaged->formatContext->nb_streams; i++) {
            if (_unmanaged->formatContext->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
                videoStreamIndex = i;
                break;
            }
        }

        if (videoStreamIndex == -1) {
            throw gcnew System::Exception("Could not find a video stream.");
        }

        // Get codec parameters for the video stream
        AVCodecParameters* codecParameters = _unmanaged->formatContext->streams[videoStreamIndex]->codecpar;

        // Find the decoder for the video stream
        const AVCodec* codec = avcodec_find_decoder(codecParameters->codec_id);
        if (!codec) {
            throw gcnew System::Exception("Unsupported codec.");
        }

        // Allocate a codec context for the decoder
        _unmanaged->codecContext = avcodec_alloc_context3(codec);
        if (!_unmanaged->formatContext) {
            throw gcnew System::Exception("Could not allocate codec context.");
        }

        // Copy codec parameters from input stream to output codec context
        if (avcodec_parameters_to_context(_unmanaged->codecContext, codecParameters) < 0) {
            throw gcnew System::Exception("Failed to copy codec parameters.");
        }

        // Open the codec
        if (avcodec_open2(_unmanaged->codecContext, codec, nullptr) < 0) {
            throw gcnew System::Exception("Could not open codec.");
        }


    }

    void FileReader::Run() {
        AVPacket packet;
        auto counter = 0;
        while (av_read_frame(_unmanaged->formatContext, &packet) >= 0) {
            // If it's the video stream and a key frame
            if (packet.stream_index == videoStreamIndex && (packet.flags & AV_PKT_FLAG_KEY)) {
                counter++;
                _callback(counter);
            }
            av_packet_unref(&packet);
        }
    }


#pragma region IDisposable
    FileReader::~FileReader() {
        /* if (disposed) {
             return;
         }
         disposed = true;
         this->!FileReader();*/
    }

    FileReader::!FileReader() {
        if (_unmanaged->codecContext) {
            avcodec_close(_unmanaged->codecContext);
            avcodec_free_context(&_unmanaged->codecContext);
        }

        if (_unmanaged->formatContext) {
            avformat_close_input(&_unmanaged->formatContext);
        }

        avformat_network_deinit();

        delete _unmanaged;
    }
#pragma endregion
}