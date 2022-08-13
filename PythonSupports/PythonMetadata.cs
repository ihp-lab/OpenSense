using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using OpenSense.Component.Contract;

namespace OpenSense.Component.PythonSupports {
    [Export(typeof(IComponentMetadata))]
    public sealed class PythonMetadata : IComponentMetadata {

        private readonly Func<IReadOnlyList<IPortMetadata>> _getPortsCallback;

        #region Constructors
        public PythonMetadata() {
            _getPortsCallback = () => Array.Empty<IPortMetadata>();
        }

        public PythonMetadata(Func<IReadOnlyList<IPortMetadata>> getPortsCallback) {
            _getPortsCallback = getPortsCallback;
        } 
        #endregion

        #region IComponentMetadata
        public string Name => "Python";

        public string Description => "Define an OpenSense component by Python3 code. Be aware of Python's performance.";

        public IReadOnlyList<IPortMetadata> Ports => _getPortsCallback();

        public ComponentConfiguration CreateConfiguration() => new PythonConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) {
            var portMetadata = this.OutputPorts()
                .Single(p => p.Identifier == portConfiguration.Identifier);
            var obj = (PythonRuntimeObject)instance;
            var producer = obj.Producers[portMetadata.Identifier];
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(producer));
            var result = HelperExtensions.CastProducerResult<T>(producer);
            return result;
        }
        #endregion
    }
}
