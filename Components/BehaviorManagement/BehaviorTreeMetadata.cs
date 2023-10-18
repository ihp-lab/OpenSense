using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Psi;

namespace OpenSense.Components.BehaviorManagement {
    [Export(typeof(IComponentMetadata))]
    public sealed class BehaviorTreeMetadata : IComponentMetadata {

        private readonly BehaviorTreeConfiguration? _configuration;

        public BehaviorTreeMetadata() {

        }

        internal BehaviorTreeMetadata(BehaviorTreeConfiguration configuration) {
            _configuration = configuration;
        }

        #region IComponentMetadata

        public string Description => "Behavior Tree for Behavior Management.";

        public string Name => "Behavior Tree";

        public IReadOnlyList<IPortMetadata> Ports => 
            GetPorts();

        public ComponentConfiguration CreateConfiguration() => 
            new BehaviorTreeConfiguration();

        public IProducer<T> GetProducer<T>(object instance, PortConfiguration portConfiguration) {
            var port = _configuration?.Root?.Ports.SingleOrDefault(p => Equals(p.Identifier, portConfiguration.Identifier));
            if (port is null) {
                throw new InvalidOperationException("Output port not exist.");
            }
            Debug.Assert(port.Direction == PortDirection.Output);
            var obj = (BehaviorTree)instance;
            var connector = obj.GetConnectorInfo(port);
            var result = HelperExtensions.CastProducerResult<T>(connector.Connector);
            return result;
        }
        #endregion

        #region Helpers
        private IReadOnlyList<IPortMetadata> GetPorts() {
            if (_configuration is null || _configuration.Root is null) {
                return Array.Empty<IPortMetadata>();
            }
            var result = _configuration.Root.Ports
                .OrderBy(p => p.Name)
                .ToArray();
            return result;
        }
        #endregion
    }
}
