using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenSense.PipelineBuilder {
    public class ComponentDescription{

        public string Name { get; set; }

        public string Description { get; set; }

        public Type ConfigurationType { get; set; }

        public PortDescription[] Inputs { get; set; } = new PortDescription[0];

        public PortDescription[] Outputs { get; set; } = new PortDescription[0];
    }
}
