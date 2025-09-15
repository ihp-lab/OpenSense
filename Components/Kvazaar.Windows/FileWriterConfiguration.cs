using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Kvazaar {
    [Serializable]
    public sealed class FileWriterConfiguration : ComponentConfiguration {

        private static readonly FileWriterMetadata Metadata = new FileWriterMetadata();

        #region Options
        private string filename = "16bit_video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool timestampFilename;

        public bool TimestampFilename {
            get => timestampFilename;
            set => SetProperty(ref timestampFilename, value);
        }
        #endregion

        #region ComponentConfiguration
        public override IComponentMetadata GetMetadata() => Metadata;

        public override object Instantiate(Pipeline pipeline, IReadOnlyList<ComponentEnvironment> instantiatedComponents, IServiceProvider? serviceProvider) {
            // Find the input connection to determine the concrete image type
            var inputPortMetadata = GetMetadata().Ports.Single(p => p.Name == nameof(FileWriter<ImageBase>.In));
            var producerMappings = this.GetRemoteProducerMappings(instantiatedComponents);
            var targetMapping = producerMappings
                .FirstOrDefault(m => Equals(m.InputConfiguration.LocalPort.Identifier, inputPortMetadata.Identifier));

            if (targetMapping is null) {
                throw new MissingRequiredInputConnectionException(Name, inputPortMetadata.Name);
            }

            // Extract the image type from Shared<TImage>
            var remoteType = targetMapping.RemoteDataType;
            if (!remoteType.IsGenericType || remoteType.GetGenericTypeDefinition() != typeof(Shared<>)) {
                throw new InvalidOperationException($"Expected Shared<TImage> but got {remoteType}");
            }

            var imageType = remoteType.GetGenericArguments()[0];
            if (!typeof(ImageBase).IsAssignableFrom(imageType)) {
                throw new InvalidOperationException($"Type {imageType} is not derived from ImageBase");
            }

            // Create the FileWriter<TImage> instance
            var fileWriterType = typeof(FileWriter<>).MakeGenericType(imageType);
            dynamic fileWriter = Activator.CreateInstance(fileWriterType, pipeline)!;

            fileWriter.Filename = Filename;
            fileWriter.TimestampFilename = TimestampFilename;
            fileWriter.Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name);

            // Connect the input port
            dynamic consumer = fileWriter.In;
            var consumerType = HelperExtensions.GetConsumerResultType(consumer);
            var getProducerFunc = typeof(HelperExtensions)
                .GetMethod(nameof(HelperExtensions.GetProducer))!
                .MakeGenericMethod(consumerType);
            dynamic producer = getProducerFunc.Invoke(null, new object[] { targetMapping.Remote, targetMapping.InputConfiguration.RemotePort });
            Microsoft.Psi.Operators.PipeTo(producer, consumer, targetMapping.InputConfiguration.DeliveryPolicy);

            return fileWriter;
        } 
        #endregion
    }
}
