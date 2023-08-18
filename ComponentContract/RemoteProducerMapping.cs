using System;

namespace OpenSense.Components.Contract {
    public sealed class RemoteProducerMapping {

        public InputConfiguration InputConfiguration { get; }

        public ComponentEnvironment Remote { get; }

        public IPortMetadata RemotePortMetadata { get; }

        public Type RemoteDataType { get; }

        public dynamic Producer { get; }

        public RemoteProducerMapping(InputConfiguration inputConfiguration, ComponentEnvironment remote, IPortMetadata remotePortMetadata, Type remoteDataType, dynamic producer) {
            InputConfiguration = inputConfiguration;
            Remote = remote;
            RemotePortMetadata = remotePortMetadata;
            RemoteDataType = remoteDataType;
            Producer = producer;
        }
    }
}
