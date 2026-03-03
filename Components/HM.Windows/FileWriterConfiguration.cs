using System;
using System.Collections.Generic;
using System.Linq;
using HMInterop;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class FileWriterConfiguration : ComponentConfiguration {

        private static readonly FileWriterMetadata Metadata = new FileWriterMetadata();

        #region Options
        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }

        private EncoderConfig raw = new EncoderConfig();

        public EncoderConfig Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }
        #endregion

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => Metadata;

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            var ports = GetMetadata().Ports;
            var inPortMetadata = ports.Single(p => p.Name == nameof(FileWriter<ImageBase>.In));
            var pictureInPortMetadata = ports.Single(p => p.Name == nameof(FileWriter<ImageBase>.PictureIn));
            var producerMappings = this.GetRemoteProducerMappings(instantiatedComponents);

            var inMapping = producerMappings.FirstOrDefault(m => Equals(m.InputConfiguration.LocalPort.Identifier, inPortMetadata.Identifier));
            var pictureInMapping = producerMappings.FirstOrDefault(m => Equals(m.InputConfiguration.LocalPort.Identifier, pictureInPortMetadata.Identifier));

            if (inMapping is null && pictureInMapping is null) {
                throw new InvalidOperationException($"At least one of {nameof(FileWriter<ImageBase>.In)} or {nameof(FileWriter<ImageBase>.PictureIn)} must be connected.");
            }

            // Resolve TImage: from In connection if available, otherwise default to ImageBase
            Type imageType;
            if (inMapping is not null) {
                var remoteType = inMapping.RemoteDataType;
                if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != typeof(Shared<>)) {
                    throw new InvalidOperationException($"Expected Shared<TImage> but got {remoteType}");
                }
                imageType = remoteType.GetGenericArguments()[0];
                if (!typeof(ImageBase).IsAssignableFrom(imageType)) {
                    throw new InvalidOperationException($"Type {imageType} is not derived from ImageBase");
                }
            } else {
                imageType = typeof(ImageBase);
            }

            // Create the FileWriter<TImage> instance
            var fileWriterType = typeof(FileWriter<>).MakeGenericType(imageType);
            dynamic fileWriter = Activator.CreateInstance(fileWriterType, pipeline)!;

            fileWriter.Filename = Filename;
            fileWriter.TimestampFilename = TimestampFilename;
            fileWriter.EncoderConfiguration = Raw.Clone();
            fileWriter.Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name);

            // Connect In port if connected
            if (inMapping is not null) {
                dynamic consumer = fileWriter.In;
                var consumerType = HelperExtensions.GetConsumerResultType(consumer);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))!
                    .MakeGenericMethod(consumerType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { inMapping.Remote, inMapping.InputConfiguration.RemotePort });
                Microsoft.Psi.Operators.PipeTo(producer, consumer, inMapping.InputConfiguration.DeliveryPolicy);
            }

            // Connect PictureIn port if connected
            if (pictureInMapping is not null) {
                dynamic consumer = fileWriter.PictureIn;
                var consumerType = HelperExtensions.GetConsumerResultType(consumer);
                var getProducerFunc = typeof(HelperExtensions)
                    .GetMethod(nameof(HelperExtensions.GetProducer))!
                    .MakeGenericMethod(consumerType);
                dynamic producer = getProducerFunc.Invoke(null, new object[] { pictureInMapping.Remote, pictureInMapping.InputConfiguration.RemotePort });
                Microsoft.Psi.Operators.PipeTo(producer, consumer, pictureInMapping.InputConfiguration.DeliveryPolicy);
            }

            return fileWriter;
        }
        #endregion
    }
}
