using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Data;

namespace OpenSense.PipelineBuilder {

    public class PipelineEnvironment {

        public Pipeline Pipeline { get; set; }

        public List<InstanceEnvironment> Instances { get; set; }
    }
}
