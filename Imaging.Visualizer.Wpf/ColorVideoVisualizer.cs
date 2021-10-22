using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenSense.Component.Imaging.Visualizer.Common;

namespace OpenSense.Component.Imaging.Visualizer {
    public class ColorVideoVisualizer : ImageVisualizer, IConsumer<Shared<Image>> {

        #region Settings

        #endregion

        #region Ports
        public Receiver<Shared<Image>> In { get; private set; }
        #endregion

        #region Binding Fields
        
        #endregion

        public ColorVideoVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
        }

        private void Process(Shared<Image> frame, Envelope envelope) {

            UpdateImage(frame, envelope.OriginatingTime);
        }
    }
}
