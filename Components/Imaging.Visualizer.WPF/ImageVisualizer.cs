using Microsoft.Psi;
using Microsoft.Psi.Imaging;

namespace OpenSense.Components.Imaging.Visualizer {
    public class ImageVisualizer : ImageHolder, IConsumer<Shared<Image>> {

        #region Settings

        #endregion

        #region Ports
        public Receiver<Shared<Image>> In { get; private set; }
        #endregion

        #region Binding Fields
        
        #endregion

        public ImageVisualizer(Pipeline pipeline) {
            In = pipeline.CreateReceiver<Shared<Image>>(this, Process, nameof(In));
        }

        private void Process(Shared<Image> frame, Envelope envelope) {

            UpdateImage(frame, envelope.OriginatingTime);
        }
    }
}
