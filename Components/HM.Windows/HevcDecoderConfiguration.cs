using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class HevcDecoderConfiguration : ConventionalComponentConfiguration {

        #region Options
        private bool processRemainingBeforeStop;

        public bool ProcessRemainingBeforeStop {
            get => processRemainingBeforeStop;
            set => SetProperty(ref processRemainingBeforeStop, value);
        } 
        #endregion

        public override IComponentMetadata GetMetadata() => new HevcDecoderMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HevcDecoder(pipeline) {
            ProcessRemainingBeforeStop = ProcessRemainingBeforeStop,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
