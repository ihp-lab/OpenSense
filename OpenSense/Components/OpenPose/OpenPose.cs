using Microsoft.Psi;
using Microsoft.Psi.Imaging;
using OpenPose;
using OpenPosePInvoke.Configuration;
using OpenPosePInvoke.Session;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace OpenSense.Components.OpenPose {
    public class OpenPose : IConsumer<Shared<Image>>, IProducer<OpenPoseDatum>, INotifyPropertyChanged {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Receiver<Shared<Image>> In { get; private set; }

        public Emitter<OpenPoseDatum> Out { get; private set; }

        private bool mute = false;

        public bool Mute {
            get => mute;
            set => SetProperty(ref mute, value);
        }

        public OpenPose(Pipeline pipeline, StaticConfiguration configuration) {
            // psi pipeline
            In = pipeline.CreateReceiver<Shared<Image>>(this, PorcessFrame, nameof(In));
            Out = pipeline.CreateEmitter<OpenPoseDatum>(this, nameof(Out));
            //openpose configuration
            Session.StaticConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            if (Session.StaticConfiguration.Input.InputType != ProducerType.None) {
                Console.WriteLine("For OpenPose to work, only input type None is supported");
                Session.StaticConfiguration.Input.InputType = ProducerType.None;
            }
            
#if DEBUG
            Session.DebugOutput = true;
#endif
            /*
			 * Want to support multiple instances?
			 * 0. Make sure OpenPose can support multiple instances
			 * 1. Wrap global variables in unityBinding by a class
			 * 2. Add an api to new class instance and pass its pointer back
			 * 3. Modify all apis to accept that pointer
			 * 4. Add an api to delete that instance when disposed
			 */
            Session.Initialize();
            pipeline.PipelineRun += OnPipeRun;
            pipeline.PipelineCompleted += OnPipeCompleted;
        }

        private void OnPipeRun(object sender, PipelineRunEventArgs e) {
            Session.Run();
        }

        private void OnPipeCompleted(object sender, PipelineCompletedEventArgs e) {
            Session.Stop();
        }

        [HandleProcessCorruptedStateExceptions]
        private void PorcessFrame(Shared<Image> frame, Envelope envelope) {
            if (Mute) {
                return;
            }
            try {
                var width = frame.Resource.Width;
                var height = frame.Resource.Height;
                var ptr = Session.AllocateNewFrameBuffer(width, height);
                var stride = width * 3;
                frame.Resource.CopyTo(ptr, width, height, stride, PixelFormat.BGR_24bpp);
                Session.PostNewFrame();
                OpenPoseDatum datum;
                do {
                    /*
					 * Want to get results asynchronously?
					 * 1. Pass time as frame name (as string) when posting new frames
					 * 2. Assign input Datum frame name field in unityBinding
					 * 3. Extract frame name field from outputs and convert back to time
					 * 4: Add a event to OPWrapper's OutputCallback method. 
					 * 5. Register emitter to that event
					 */
                    Task.Delay(10);
                } while (!Session.GetOutput(out datum));
                Out.Post(datum, envelope.OriginatingTime);
            } catch (Exception ex) {
                Console.Error.WriteLine($"An exception is raised in {GetType().Name}: {ex.ToString()}");
                Mute = true;
            }
        }
    }


}
