using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using OpenSense.Component.Contract;

namespace OpenSense.Component.Imaging.Visualizer {
    [Serializable]
    public class ColorVideoVisualizerConfiguration : ConventionalComponentConfiguration {

        public override IComponentMetadata GetMetadata() => new ColorVideoVisualizerMetadata();

        protected override object Instantiate(Pipeline pipeline) => new ColorVideoVisualizer(pipeline);
    }
}
