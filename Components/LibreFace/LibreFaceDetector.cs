using System;
using System.Collections.Generic;
using System.Linq;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.LibreFace {
    public sealed class LibreFaceDetector : Subpipeline{

        private readonly Connector<IReadOnlyList<NormalizedLandmarkList>> _inConnector;
        private readonly Connector<Shared<Image>> _imageInConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _auOutConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _feOutConnector;
        private readonly Connector<IReadOnlyList<Shared<Image>>> _alignedImagesOutConnector;

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => _inConnector.In;
        public Receiver<Shared<Image>> ImageIn => _imageInConnector.In;

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> ActionUnitOut => _auOutConnector.Out;
        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> FacialExpressionOut => _feOutConnector.Out;
        public Emitter<IReadOnlyList<Shared<Image>>> AlignedImagesOut => _alignedImagesOutConnector.Out;

        public LibreFaceDetector(Pipeline pipeline) : base(pipeline, nameof(LibreFaceDetector), DeliveryPolicy.LatestMessage) {
            _inConnector = CreateInputConnectorFrom<IReadOnlyList<NormalizedLandmarkList>>(pipeline, nameof(DataIn));
            _imageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));

            _auOutConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<string, float>>>(pipeline, nameof(ActionUnitOut));
            _feOutConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<string, float>>>(pipeline, nameof(FacialExpressionOut));
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

            var auInferenceRunner = new ActionUnitInferenceRunner(this);
            aligner.PipeTo(auInferenceRunner, DeliveryPolicy.LatestMessage);
            auInferenceRunner.PipeTo(_auOutConnector, DeliveryPolicy.LatestMessage);

            var feInferenceRunner = new FacialExpressionInferenceRunner(this);
            aligner.PipeTo(feInferenceRunner, DeliveryPolicy.LatestMessage);
            feInferenceRunner.PipeTo(_feOutConnector, DeliveryPolicy.LatestMessage);
        }
    }
}
