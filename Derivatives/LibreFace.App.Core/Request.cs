using Microsoft.Psi.Imaging;

namespace LibreFace.App {
    internal struct Request { 

        public Image Image { get; }

        public TaskCompletionSource<(ActionUnitPresenceOutput, ActionUnitIntensityOutput, ExpressionOutput)> TaskCompletionSource { get; } = new();

        public Request(Image image) {
            Image = image;
        }
    }
}
