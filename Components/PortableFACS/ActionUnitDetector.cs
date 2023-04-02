using System;
using System.Collections.Generic;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.PortableFACS {
    public sealed class ActionUnitDetector : Subpipeline, IProducer<IReadOnlyDictionary<int, float>> {

        private readonly Connector<IReadOnlyList<NormalizedLandmarkList>> _dataInConnector;
        private readonly Connector<Shared<Image>> _imageInConnector;
        private readonly Connector<IReadOnlyDictionary<int, float>> _outConnector;

        private readonly FaceImageAligner _aligner;

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => _dataInConnector.In;
        public Receiver<Shared<Image>> ImageIn => _imageInConnector.In;

        public Emitter<IReadOnlyDictionary<int, float>> Out => _outConnector.Out;

        public ActionUnitDetector(Pipeline pipeline) : base(pipeline, nameof(ActionUnitDetector), DeliveryPolicy.Unlimited) {
            _dataInConnector = pipeline.CreateConnector<IReadOnlyList<NormalizedLandmarkList>>(nameof(DataIn));
            _imageInConnector = pipeline.CreateConnector<Shared<Image>>(nameof(ImageIn));
            _outConnector = pipeline.CreateConnector<IReadOnlyDictionary<int, float>>(nameof(Out));
            _aligner = new FaceImageAligner(pipeline);

            _dataInConnector.Join(_imageInConnector).PipeTo(_aligner);
        }
    }
}
