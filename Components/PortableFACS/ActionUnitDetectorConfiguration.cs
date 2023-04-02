using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using OpenSense.Components.Contract;

namespace OpenSense.Components.PortableFACS {
    [Serializable]
    public class ActionUnitDetectorConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ActionUnitDetectorMetadata();

        protected override object Instantiate(Pipeline pipeline, IServiceProvider serviceProvider) => new ActionUnitDetector(pipeline) {
            
        };
    }
}
