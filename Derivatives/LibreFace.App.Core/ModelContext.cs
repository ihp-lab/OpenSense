using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Psi.Imaging;

namespace LibreFace.App {
    public sealed class ModelContext : IDisposable {

        private static readonly ChannelOptions Options = new UnboundedChannelOptions() {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        };

        private readonly SessionOptions _options;

        private readonly ActionUnitEncoderModelContext _encoder;

        private readonly ActionUnitPresenceModelContext _presence;

        private readonly ActionUnitIntensityModelContext _intensity;

        private readonly FacialExpressionModelContext _expression;

        private readonly Channel<Request> _requests = Channel.CreateUnbounded<Request>();

        private readonly Task _task;

        public ModelContext(int cudaDeviceId) {
            if (cudaDeviceId >= 0) {
                _options = SessionOptions.MakeSessionOptionWithCudaProvider(cudaDeviceId);
            } else { 
                _options = new SessionOptions();
            }
            
            _encoder = new ActionUnitEncoderModelContext(_options, isOwner: true);
            _presence = new ActionUnitPresenceModelContext(_options, isOwner: true);
            _intensity = new ActionUnitIntensityModelContext(_options, isOwner: true);
            _expression = new FacialExpressionModelContext(_options, isOwner: true);

            _task = Run();
        }

        public TaskCompletionSource<(ActionUnitPresenceOutput, ActionUnitIntensityOutput, ExpressionOutput)> Run(Image image) {
            var request = new Request(image);
            var flag = _requests.Writer.TryWrite(request);
            Debug.Assert(flag);
            return request.TaskCompletionSource;
        }

        private async Task Run() { 
            while (true) {
                var hasNext = await _requests.Reader.WaitToReadAsync();
                if (!hasNext) {
                    break;
                }
                var request = await _requests.Reader.ReadAsync();
                Process(request);
            }
        }

        private void Process(Request request) {
            var img = request.Image;
            unsafe {
                var span = new Span<byte>(img.ImageData.ToPointer(), img.Size);
                using var input = new ImageInput(span, img.Stride);
                var features = _encoder.Run(input);
                var pResult = _presence.Run(features);
                var iResult = _intensity.Run(features);
                var fResult = _expression.Run(input);
                request.TaskCompletionSource.SetResult((pResult, iResult, fResult));
            }
        }

        #region IDisposable
        private bool disposed;

        public void Dispose() { 
            if (disposed) {
                return;
            }
            disposed = true;

            _requests.Writer.Complete();
            _task.Wait();
            _expression.Dispose();
            _intensity.Dispose();
            _presence.Dispose();
            _encoder.Dispose();
            _options.Dispose();
        }
        #endregion
    }
}
