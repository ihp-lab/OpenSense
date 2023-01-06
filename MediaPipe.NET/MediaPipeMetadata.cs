#nullable enable

using System;
using System.Collections.Generic;
using System.Composition;
using OpenSense.Component.Contract;

namespace OpenSense.Component.MediaPipe.NET {
    [Export(typeof(IComponentMetadata))]
    public class MediaPipeMetadata : IComponentMetadata {

        private readonly Func<IReadOnlyList<IPortMetadata>> _getPortsCallback;

        #region Constructors
        public MediaPipeMetadata() {
            _getPortsCallback = () => Array.Empty<IPortMetadata>();
        }

        internal MediaPipeMetadata(Func<IReadOnlyList<IPortMetadata>> getPortsCallback) {
            _getPortsCallback = getPortsCallback;
        }
        #endregion

        public string Name => "MediaPipe";

        public string Description => "[Experimental] Runs a MediaPipe pipeline. The backend is MediaPipe.Net and it's CPU only.";

        public IReadOnlyList<IPortMetadata> Ports => _getPortsCallback();

        public ComponentConfiguration CreateConfiguration() => new MediaPipeConfiguration();

        public object GetConnector<T>(object instance, PortConfiguration portConfiguration) {
            var pipe = (SolutionWrapper)instance;
            var key = (string)portConfiguration.Identifier;
            var producer = pipe.Outputs[key];
            var result = HelperExtensions.CastProducerResult<T>(producer);
            return result;
        }
    }
}
