using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using OpenSense.Components.Contract;

namespace OpenSense.Components.PythonSupports {
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
        public string Name => "Python 3";

        public string Description => "Uses Python 3 codes as a OpenSense component. The backend is IronPython3, and it supports Python 3.4 features.";

        public IReadOnlyList<IPortMetadata> Ports => _getPortsCallback();

        public ComponentConfiguration CreateConfiguration() => new PythonConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) {
            var portMetadata = this.OutputPorts()
                .Single(p => Equals(p.Identifier, portConfiguration.Identifier));//Do not use ==
            var obj = (PythonRuntimeObject)instance;
            var producer = obj.Producers[portMetadata.Identifier];
            Debug.Assert(instance != null && HelperExtensions.CanProducerResultBeCastTo<T>(producer));
            var result = HelperExtensions.CastProducerResult<T>(producer);
            return result;
        }
        #endregion
    }
}
