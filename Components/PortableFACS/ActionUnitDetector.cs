using System;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.PortableFACS {
    public sealed class ActionUnitDetector : Subpipeline, IProducer<IReadOnlyList<IReadOnlyDictionary<int, float>>> {

        private readonly Connector<IReadOnlyList<NormalizedLandmarkList>> _inConnector;
        private readonly Connector<Shared<Image>> _imageInConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<int, float>>> _outConnector;
        private readonly Connector<IReadOnlyList<Shared<Image>>> _alignedImagesOutConnector;

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => _inConnector.In;
        public Receiver<Shared<Image>> ImageIn => _imageInConnector.In;

        public Emitter<IReadOnlyList<IReadOnlyDictionary<int, float>>> Out => _outConnector.Out;
        public Emitter<IReadOnlyList<Shared<Image>>> AlignedImagesOut => _alignedImagesOutConnector.Out;

        public ActionUnitDetector(Pipeline pipeline) : base(pipeline, nameof(ActionUnitDetector), DeliveryPolicy.LatestMessage) {
            _inConnector = CreateInputConnectorFrom<IReadOnlyList<NormalizedLandmarkList>>(pipeline, nameof(DataIn));
            _imageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));

            _outConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<int, float>>>(pipeline, nameof(Out));
            _alignedImagesOutConnector = CreateOutputConnectorTo<IReadOnlyList<Shared<Image>>>(pipeline, nameof(AlignedImagesOut));

            var convertedImage = _imageInConnector
                .Convert(PixelFormat.RGB_24bpp, DeliveryPolicy.LatestMessage);
            var aligner = new FaceImageAligner(this);
            _inConnector.Join(
                    convertedImage,
                    Reproducible.Exact<Shared<Image>>(),
                    ValueTuple.Create,
                    DeliveryPolicy.LatestMessage,
                    DeliveryPolicy.LatestMessage
                )//TODO: shared image pool grows too large when no face is detected! Fuse() does not help; swapping streams does not help.
                .PipeTo(aligner, DeliveryPolicy.LatestMessage);
            aligner.PipeTo(_alignedImagesOutConnector, DeliveryPolicy.LatestMessage);

            var inferenceRunner = new InferenceRunner(this);
            aligner.PipeTo(inferenceRunner, DeliveryPolicy.LatestMessage);
            inferenceRunner.PipeTo(_outConnector, DeliveryPolicy.LatestMessage);
        }
    }
}
