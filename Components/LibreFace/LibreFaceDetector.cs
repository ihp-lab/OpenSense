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
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _auIntensityOutConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, bool>>> _auPresenceOutConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _feOutConnector;
        private readonly Connector<IReadOnlyList<Shared<Image>>> _alignedImagesOutConnector;

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => _inConnector.In;
        public Receiver<Shared<Image>> ImageIn => _imageInConnector.In;

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> ActionUnitIntensityOut => _auIntensityOutConnector.Out;
        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>> ActionUnitPresenceOut => _auPresenceOutConnector.Out;
        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> FacialExpressionOut => _feOutConnector.Out;
        public Emitter<IReadOnlyList<Shared<Image>>> AlignedImagesOut => _alignedImagesOutConnector.Out;

        public LibreFaceDetector(Pipeline pipeline, DeliveryPolicy deliveryPolicy) : base(pipeline, nameof(LibreFaceDetector), deliveryPolicy) {
            _inConnector = CreateInputConnectorFrom<IReadOnlyList<NormalizedLandmarkList>>(pipeline, nameof(DataIn));
            _imageInConnector = CreateInputConnectorFrom<Shared<Image>>(pipeline, nameof(ImageIn));

            _auIntensityOutConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<string, float>>>(pipeline, nameof(ActionUnitIntensityOut));
            _auPresenceOutConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<string, bool>>>(pipeline, nameof(ActionUnitPresenceOut));
            _feOutConnector = CreateOutputConnectorTo<IReadOnlyList<IReadOnlyDictionary<string, float>>>(pipeline, nameof(FacialExpressionOut));
            _alignedImagesOutConnector = CreateOutputConnectorTo<IReadOnlyList<Shared<Image>>>(pipeline, nameof(AlignedImagesOut));

            var convertedImage = _imageInConnector
                .Convert(PixelFormat.RGB_24bpp, deliveryPolicy);
            var aligner = new FaceImageAligner(this);
            _inConnector.Join(
                    convertedImage,
                    Reproducible.Exact<Shared<Image>>(),
                    ValueTuple.Create,
                    deliveryPolicy,
                    deliveryPolicy
                )//TODO: shared image pool grows too large when no face is detected! Fuse() does not help; swapping streams does not help.
                .PipeTo(aligner, deliveryPolicy);
            aligner.PipeTo(_alignedImagesOutConnector, deliveryPolicy);

            var auIntensitiyInferenceRunner = new ActionUnitIntensityInferenceRunner(this);
            aligner.PipeTo(auIntensitiyInferenceRunner, deliveryPolicy);
            auIntensitiyInferenceRunner.PipeTo(_auIntensityOutConnector, deliveryPolicy);

            var auPresenceInferenceRunner = new ActionUnitPresenceInferenceRunner(this);
            aligner.PipeTo(auPresenceInferenceRunner, deliveryPolicy);
            auPresenceInferenceRunner.PipeTo(_auPresenceOutConnector, deliveryPolicy);

            var feInferenceRunner = new FacialExpressionInferenceRunner(this);
            aligner.PipeTo(feInferenceRunner, deliveryPolicy);
            feInferenceRunner.PipeTo(_feOutConnector, deliveryPolicy);
        }
    }
}
