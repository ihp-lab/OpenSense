#include "pch.h"
#include "PictureYuvPool.h"

namespace HMInterop {
    PictureYuv^ PictureYuvPool::Rent(ChromaFormat chromaFormat, int width, int height) {
        auto key = ValueTuple<int, int, int>(width, height, static_cast<int>(chromaFormat));
        ConcurrentQueue<PictureYuv^>^ queue;
        if (pools->TryGetValue(key, queue)) {
            PictureYuv^ result;
            if (queue->TryDequeue(result)) {
                return result;
            }
        }
        return gcnew PictureYuv(chromaFormat, width, height);
    }

    void PictureYuvPool::Return(PictureYuv^ picYuv) {
        if (picYuv == nullptr) {
            return;
        }
        auto key = ValueTuple<int, int, int>(
            picYuv->Width, picYuv->Height, static_cast<int>(picYuv->ChromaFormat));
        auto queue = pools->GetOrAdd(key, gcnew ConcurrentQueue<PictureYuv^>());
        queue->Enqueue(picYuv);
    }
}
