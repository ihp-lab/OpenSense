﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mediapipe.Net.Framework.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.LibreFace {
    public sealed class LibreFaceDetector : Subpipeline, INotifyPropertyChanged {

        private readonly Connector<IReadOnlyList<NormalizedLandmarkList>> _inConnector;
        private readonly Connector<Shared<Image>> _imageInConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _auIntensityOutConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, bool>>> _auPresenceOutConnector;
        private readonly Connector<IReadOnlyList<IReadOnlyDictionary<string, float>>> _feOutConnector;
        private readonly Connector<IReadOnlyList<Shared<Image>>> _alignedImagesOutConnector;

        #region Settings

        private ILogger? logger;

        public ILogger? Logger {
            get => logger;
            set => SetProperty(ref logger, value);
        }
        #endregion

        public Receiver<IReadOnlyList<NormalizedLandmarkList>> DataIn => _inConnector.In;
        public Receiver<Shared<Image>> ImageIn => _imageInConnector.In;

        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> ActionUnitIntensityOut => _auIntensityOutConnector.Out;
        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, bool>>> ActionUnitPresenceOut => _auPresenceOutConnector.Out;
        public Emitter<IReadOnlyList<IReadOnlyDictionary<string, float>>> FacialExpressionOut => _feOutConnector.Out;
        public Emitter<IReadOnlyList<Shared<Image>>> AlignedImagesOut => _alignedImagesOutConnector.Out;

        public LibreFaceDetector(
            Pipeline pipeline, 
            DeliveryPolicy deliveryPolicy, 
            bool auIntensity = true, 
            bool auPresence = true, 
            bool facialExpression = true
            ) : base(pipeline, nameof(LibreFaceDetector), deliveryPolicy) {
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

            if (auIntensity || auPresence) {
                var auInferenceRunner = new ActionUnitInferenceRunner(this, auIntensity, auPresence);
                aligner.PipeTo(auInferenceRunner, deliveryPolicy);
                auInferenceRunner.IntensityOut.PipeTo(_auIntensityOutConnector, deliveryPolicy);
                auInferenceRunner.PresenceOut.PipeTo(_auPresenceOutConnector, deliveryPolicy);
            }

            if (facialExpression) {
                var feInferenceRunner = new FacialExpressionInferenceRunner(this);
                aligner.PipeTo(feInferenceRunner, deliveryPolicy);
                feInferenceRunner.PipeTo(_feOutConnector, deliveryPolicy); 
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
