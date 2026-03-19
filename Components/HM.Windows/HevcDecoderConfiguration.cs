using System;
using Microsoft.Extensions.Logging;
using Microsoft.Psi;

namespace OpenSense.Components.HM {
    [Serializable]
    public sealed class HevcDecoderConfiguration : ConventionalComponentConfiguration {

        #region Options
        private bool discardRemainingOnStop;

        public bool DiscardRemainingOnStop {
            get => discardRemainingOnStop;
            set => SetProperty(ref discardRemainingOnStop, value);
        } 
        #endregion

        public override IComponentMetadata GetMetadata() => new HevcDecoderMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new HevcDecoder(pipeline) {
            DiscardRemainingOnStop = DiscardRemainingOnStop,
            Logger = (serviceProvider?.GetService(typeof(ILoggerFactory)) as ILoggerFactory)?.CreateLogger(Name),
        };
    }
}
