using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Psi;
using Microsoft.Psi.Audio;
using Microsoft.Psi.Media;
using Microsoft.Psi.AzureKinect;
using Microsoft.Psi.CognitiveServices.Speech;
using Microsoft.Psi.Speech;
using System.Linq;
using Microsoft.Psi.Data;
using Microsoft.Psi.Remoting;
using Microsoft.Psi.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#if NETFRAMEWORK
using Microsoft.Psi.Kinect;
#endif

namespace OpenSense.PipelineBuilder.Configurations {
    #region generator
    public class MediaCaptureConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Media.MediaCaptureConfiguration raw = new Microsoft.Psi.Media.MediaCaptureConfiguration();

        public Microsoft.Psi.Media.MediaCaptureConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new MediaCapture(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(MediaCapture).FullName,
            Description = "Component that captures and streams video and audio from a camera.",
            ConfigurationType = typeof(MediaCaptureConfiguration),
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(MediaCapture).GetProperty(nameof(MediaCapture.Video)),
                },
                new StaticPortDescription() {
                    Property = typeof(MediaCapture).GetProperty(nameof(MediaCapture.Audio)),
                },
            }
        };
    }

    public class AudioCaptureConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Audio.AudioCaptureConfiguration raw = new Microsoft.Psi.Audio.AudioCaptureConfiguration();

        public Microsoft.Psi.Audio.AudioCaptureConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new AudioCapture(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AudioCapture).FullName,
            Description = "Component that captures and streams audio from an input device such as a microphone.",
            ConfigurationType = typeof(AudioCaptureConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AudioCapture).GetProperty(nameof(AudioCapture.AudioLevelInput)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AudioCapture).GetProperty(nameof(AudioCapture.Out)),
                },
                new StaticPortDescription() {
                    Property = typeof(AudioCapture).GetProperty(nameof(AudioCapture.AudioLevel)),
                },
            },
        };
    }

    public class AzureKinectSensorConfiguration : ComponentConfiguration {

        private Microsoft.Psi.AzureKinect.AzureKinectSensorConfiguration raw = new Microsoft.Psi.AzureKinect.AzureKinectSensorConfiguration();

        public Microsoft.Psi.AzureKinect.AzureKinectSensorConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public DeliveryPolicy DefaultDeliveryPolicy { get; set; } = null;

        public DeliveryPolicy BodyTrackerDeliveryPolicy { get; set; } = null;

        public override object Instantiate(Pipeline pipeline) => new AzureKinectSensor(pipeline, Raw, DefaultDeliveryPolicy, BodyTrackerDeliveryPolicy);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AzureKinectSensor).FullName,
            Description = "Component that captures all sensor streams and tracked bodies from the Azure Kinect device.",
            ConfigurationType = typeof(AzureKinectSensorConfiguration),
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.ColorImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.InfraredImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.DepthImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.FrameRate)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.Imu)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.DepthDeviceCalibrationInfo)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.AzureKinectSensorCalibration)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.Temperature)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectSensor).GetProperty(nameof(AzureKinectSensor.Bodies)),
                },
            },
        };
    }

#if NETFRAMEWORK
    public class SystemVoiceActivityDetectorConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Speech.SystemVoiceActivityDetectorConfiguration raw = new Microsoft.Psi.Speech.SystemVoiceActivityDetectorConfiguration();

        public Microsoft.Psi.Speech.SystemVoiceActivityDetectorConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new SystemVoiceActivityDetector(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(SystemVoiceActivityDetector).FullName,
            Description = "Component that performs voice activity detection by using the desktop speech recognition engine from `System.Speech`.",
            ConfigurationType = typeof(SystemVoiceActivityDetectorConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(SystemVoiceActivityDetector).GetProperty(nameof(SystemVoiceActivityDetector.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(SystemVoiceActivityDetector).GetProperty(nameof(SystemVoiceActivityDetector.Out)),
                },
            },
        };
    }

    public class SystemSpeechRecognizerConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration raw = new Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration();

        public Microsoft.Psi.Speech.SystemSpeechRecognizerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new SystemSpeechRecognizer(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(SystemSpeechRecognizer).FullName,
            Description = "Component that performs speech recognition using the desktop speech recognition engine from `System.Speech`.",
            ConfigurationType = typeof(SystemSpeechRecognizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.In)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.ReceiveGrammars)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.PartialRecognitionResults)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.EmulateRecognizeCompleted)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.AudioLevelUpdated)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.RecognizeCompleted)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.AudioStateChanged)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.AudioSignalProblemOccurred)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.SpeechRecognitionRejected)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.SpeechRecognized)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.SpeechHypothesized)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.SpeechDetected)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.IntentData)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.LoadGrammarCompleted)),
                },
                new StaticPortDescription() {
                    Property = typeof(SystemSpeechRecognizer).GetProperty(nameof(SystemSpeechRecognizer.RecognizerUpdateReached)),
                },
            },
        };
    }

    public class KinectSensorConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Kinect.KinectSensorConfiguration raw = Microsoft.Psi.Kinect.KinectSensorConfiguration.Default;

        public Microsoft.Psi.Kinect.KinectSensorConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new KinectSensor(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(KinectSensor).FullName,
            Description = "Component that captures and streams information (images, depth, audio, bodies, etc.) from a Kinect One (v2) sensor.",
            ConfigurationType = typeof(KinectSensorConfiguration),
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.AudioBodyCorrelations)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.AudioBeamInfo)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.Audio)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.DepthDeviceCalibrationInfo)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.DepthFrameToCameraSpaceTable)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.LongExposureInfraredImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.InfraredImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.RGBDImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.ColorToCameraMapper)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.ColorImage)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.Bodies)),
                },
                new StaticPortDescription() {
                    Property = typeof(KinectSensor).GetProperty(nameof(KinectSensor.DepthImage)),
                },
            },
        };
    }
#endif

    public class MediaSourceConfiguration : ComponentConfiguration {

        private string filename;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private bool dropOutOfOrderPackets = false;

        public bool DropOutOfOrderPackets {
            get => dropOutOfOrderPackets;
            set => SetProperty(ref dropOutOfOrderPackets, value);
        }

        public override object Instantiate(Pipeline pipeline) => new MediaSource(pipeline, Filename, DropOutOfOrderPackets);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(MediaSource).FullName,
            Description = "Component that streams video and audio from a media file.",
            ConfigurationType = typeof(MediaSourceConfiguration),
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(MediaSource).GetProperty(nameof(MediaSource.Image)),
                },
                new StaticPortDescription() {
                    Property = typeof(MediaSource).GetProperty(nameof(MediaSource.Audio)),
                },
            }
        };
    }

    public class WaveFileAudioSourceConfiguration : ComponentConfiguration {

        private string filename;

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private DateTime? audioStartTime = null;

        public DateTime? AudioStartTime {
            get => audioStartTime;
            set => SetProperty(ref audioStartTime, value);
        }

        private int targetLatencyMs = 20;

        public int TargetLatencyMs {
            get => targetLatencyMs;
            set => SetProperty(ref targetLatencyMs, value);
        }

        public override object Instantiate(Pipeline pipeline) => new WaveFileAudioSource(pipeline, Filename, AudioStartTime, TargetLatencyMs);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(WaveFileAudioSource).FullName,
            Description = "Initializes a new instance of the Microsoft.Psi.Audio.WaveFileAudioSource class.",
            ConfigurationType = typeof(WaveFileAudioSourceConfiguration),
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(WaveFileAudioSource).GetProperty(nameof(WaveFileAudioSource.Out)),
                },
            }
        };
    }
    #endregion

    #region component
    public class AudioResamplerConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Audio.AudioResamplerConfiguration raw = new Microsoft.Psi.Audio.AudioResamplerConfiguration();

        public Microsoft.Psi.Audio.AudioResamplerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new AudioResampler(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AudioResampler).FullName,
            Description = "Component that resamples an audio stream into a different format.",
            ConfigurationType = typeof(AudioResamplerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AudioResampler).GetProperty(nameof(AudioResampler.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AudioResampler).GetProperty(nameof(AudioResampler.Out)),
                },
            }
        };
    }

    public class AzureSpeechRecognizerConfiguration : ComponentConfiguration {

        private Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration raw = new Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration();

        public Microsoft.Psi.CognitiveServices.Speech.AzureSpeechRecognizerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new AzureSpeechRecognizer(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AzureSpeechRecognizer).FullName,
            Description = "Component that performs speech recognition using the Microsoft Cognitive Services Azure Speech API.",
            ConfigurationType = typeof(AzureSpeechRecognizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureSpeechRecognizer).GetProperty(nameof(AzureSpeechRecognizer.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureSpeechRecognizer).GetProperty(nameof(AzureSpeechRecognizer.PartialRecognitionResults)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureSpeechRecognizer).GetProperty(nameof(AzureSpeechRecognizer.PartialSpeechResponseEvent)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureSpeechRecognizer).GetProperty(nameof(AzureSpeechRecognizer.SpeechErrorEvent)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureSpeechRecognizer).GetProperty(nameof(AzureSpeechRecognizer.SpeechResponseEvent)),
                },
            },
        };
    }

    public class AzureKinectBodyTrackerConfiguration : ComponentConfiguration {

        private Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration raw = new Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration();

        public Microsoft.Psi.AzureKinect.AzureKinectBodyTrackerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new AzureKinectBodyTracker(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AzureKinectBodyTracker).FullName,
            Description = "Component that performs body tracking from the depth/IR images captured by the Azure Kinect sensor.",
            ConfigurationType = typeof(AzureKinectBodyTrackerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTracker).GetProperty(nameof(AzureKinectBodyTracker.In)),
                },
                new StaticPortDescription() {
                    Property  = typeof(AzureKinectBodyTracker).GetProperty(nameof(AzureKinectBodyTracker.AzureKinectSensorCalibration)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTracker).GetProperty(nameof(AzureKinectBodyTracker.Out)),
                },
            },
        };
    }

    public class Mpeg4WriterConfiguration : ComponentConfiguration {

        private string filename = "video.mp4";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        private Microsoft.Psi.Media.Mpeg4WriterConfiguration raw = Microsoft.Psi.Media.Mpeg4WriterConfiguration.Default;

        public Microsoft.Psi.Media.Mpeg4WriterConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new Mpeg4Writer(pipeline, Filename, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Mpeg4Writer).FullName,
            Description = "Component that writes video and audio streams into an MPEG-4 file.",
            ConfigurationType = typeof(Mpeg4WriterConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Mpeg4Writer).GetProperty(nameof(Mpeg4Writer.ImageIn)),
                },
                new StaticPortDescription() {
                    Property  = typeof(Mpeg4Writer).GetProperty(nameof(Mpeg4Writer.AudioIn)),
                },
            },
        };
    }

    public class WaveFileWriterConfiguration : ComponentConfiguration {

        private string filename = "audio.wav";

        public string Filename {
            get => filename;
            set => SetProperty(ref filename, value);
        }

        public override object Instantiate(Pipeline pipeline) => new WaveFileWriter(pipeline, Filename);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(WaveFileWriter).FullName,
            Description = "Component that writes an audio stream into a WAVE file.",
            ConfigurationType = typeof(WaveFileWriterConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(WaveFileWriter).GetProperty(nameof(WaveFileWriter.In)),
                },
            },
        };
    }

    public class AudioPlayerConfiguration : ComponentConfiguration {

        private Microsoft.Psi.Audio.AudioPlayerConfiguration raw = new Microsoft.Psi.Audio.AudioPlayerConfiguration();

        public Microsoft.Psi.Audio.AudioPlayerConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new AudioPlayer(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AudioPlayer).FullName,
            Description = "Component that plays back an audio stream to an output device such as the speakers.",
            ConfigurationType = typeof(AudioPlayerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(AudioPlayer).GetProperty(nameof(AudioPlayer.In)),
                },
                new StaticPortDescription() {
                    Property  = typeof(AudioPlayer).GetProperty(nameof(AudioPlayer.AudioLevelInput)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(AudioPlayer).GetProperty(nameof(AudioPlayer.AudioLevel)),
                },
            }
        };
    }

    public enum PsiBuiltinImageToStreamEncoder {
        Png,
        Jpeg,
    }

    public class ImageEncoderConfiguration : ComponentConfiguration {

        

        private PsiBuiltinImageToStreamEncoder encoder = PsiBuiltinImageToStreamEncoder.Png;

        [JsonConverter(typeof(StringEnumConverter))]
        public PsiBuiltinImageToStreamEncoder Encoder {
            get => encoder;
            set => SetProperty(ref encoder, value);
        }

        private int qualityLevel = 100;

        public int QualityLevel {
            get => qualityLevel;
            set => SetProperty(ref qualityLevel, value);
        }

        private IImageToStreamEncoder CreateEncoder() {
            switch (Encoder) {
                case PsiBuiltinImageToStreamEncoder.Png:
                    return new ImageToPngStreamEncoder();
                case PsiBuiltinImageToStreamEncoder.Jpeg:
                    return new ImageToJpegStreamEncoder() { QualityLevel = QualityLevel };
                default:
                    throw new InvalidOperationException();
            }
        }

        public override object Instantiate(Pipeline pipeline) => new ImageEncoder(pipeline, CreateEncoder());

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(ImageEncoder).FullName,
            Description = "Component that encodes an image using a specified Microsoft.Psi.Imaging.IImageToStreamEncoder.",
            ConfigurationType = typeof(ImageEncoderConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(ImageEncoder).GetProperty(nameof(ImageEncoder.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(ImageEncoder).GetProperty(nameof(ImageEncoder.Out)),
                },
            }
        };
    }

    public class ImageDecoderConfiguration : ComponentConfiguration {

        private IImageFromStreamDecoder CreateDecoder() {
            return new ImageFromStreamDecoder();
        }

        public override object Instantiate(Pipeline pipeline) => new ImageDecoder(pipeline, CreateDecoder());

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(ImageDecoder).FullName,
            Description = "Component that decodes an image using Microsoft.Psi.Imaging.ImageFromStreamDecoder.",
            ConfigurationType = typeof(ImageDecoderConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(ImageDecoder).GetProperty(nameof(ImageDecoder.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property  = typeof(ImageDecoder).GetProperty(nameof(ImageDecoder.Out)),
                },
            }
        };
    }

    #endregion

    #region operators
    public class JoinOperatorConfiguration : OperatorConfiguration {

        private DeliveryPolicy primaryDeliveryPolicy;

        public DeliveryPolicy PrimaryDeliveryPolicy {
            get => primaryDeliveryPolicy;
            set => SetProperty(ref primaryDeliveryPolicy, value);
        }

        private DeliveryPolicy secondaryDeliveryPolicy;

        public DeliveryPolicy SecondaryDeliveryPolicy {
            get => secondaryDeliveryPolicy;
            set => SetProperty(ref secondaryDeliveryPolicy, value);
        }

        public override object Instantiate(IList<dynamic> inputs) {
            if (inputs.Count != 2) {
                throw new ArgumentException("not enough inputs", nameof(inputs));
            }
            return Microsoft.Psi.Operators.Join(inputs[0], inputs[1], PrimaryDeliveryPolicy, SecondaryDeliveryPolicy);
        }

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Join Operator",
            Description = "Join the primary stream with values from a secondary stream.",
            ConfigurationType = typeof(JoinOperatorConfiguration),
            Inputs = new [] {
                new VirtualPortDescription() {
                    VirtualName = "Primary",
                    PortType = PortType.Input,
                },
                new VirtualPortDescription() {
                    VirtualName = "Secondary",
                    PortType = PortType.Input,
                },
            },
            Outputs = new [] {
                new VirtualPortDescription() {
                    VirtualName = "Out",
                    PortType = PortType.Output,
                },
            },
        };
    }
    #endregion

    #region exporter
    public class LocalExporterConfiguration : ExporterConfiguration {

        private string storeName = string.Empty;

        public string StoreName {
            get => storeName;
            set => SetProperty(ref storeName, value);
        }

        private string rootPath;

        public string RootPath {
            get => rootPath;
            set => SetProperty(ref rootPath, value);
        }

        private bool createSubdirectory;

        public bool CreateSubdirectory {
            get => createSubdirectory;
            set => SetProperty(ref createSubdirectory, value);
        }

        public override object Instantiate(Pipeline pipeline) => Store.Create(pipeline, StoreName, RootPath, CreateSubdirectory);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Local Exporter",
            Description = "Save data at a local store.",
            ConfigurationType = typeof(LocalExporterConfiguration),
        };
    }

    public class RemoteExporterConfiguration : ExporterConfiguration {

        private int port = 11411;

        public int Port {
            get => port;
            set => SetProperty(ref port, value);
        }

        private TransportKind transport = TransportKind.NamedPipes;

        public TransportKind Transport {
            get => transport;
            set => SetProperty(ref transport, value);
        }

        private long maxBytesPerSecond = long.MaxValue;

        public long MaxBytesPerSecond {
            get => maxBytesPerSecond;
            set => SetProperty(ref maxBytesPerSecond, value);
        }

        private double bytesPerSecondSmoothingWindowSeconds = 5;

        public double BytesPerSecondSmoothingWindowSeconds {
            get => bytesPerSecondSmoothingWindowSeconds;
            set => SetProperty(ref bytesPerSecondSmoothingWindowSeconds, value);
        }

        public override object Instantiate(Pipeline pipeline) => new RemoteExporter(pipeline, Port, Transport, MaxBytesPerSecond, BytesPerSecondSmoothingWindowSeconds);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Remote Exporter",
            Description = "Send data through the network.",
            ConfigurationType = typeof(RemoteExporterConfiguration),
        };
    }
    #endregion

    #region stream writer

    public class StreamWriterImplConfiguration : StreamWriterConfiguration {

        private Guid exporter;

        public Guid Exporter {
            get => exporter;
            set => SetProperty(ref exporter, value);
        }

        private string streamName = string.Empty;

        public string StreamName {
            get => streamName;
            set => SetProperty(ref streamName, value);
        }

        private bool largeMessage;

        public bool LargeMessage {
            get => largeMessage;
            set => SetProperty(ref largeMessage, value);
        }

        private DeliveryPolicy deliveryPolicy = DeliveryPolicy.LatestMessage;

        public DeliveryPolicy DeliveryPolicy {
            get => deliveryPolicy;
            set => SetProperty(ref deliveryPolicy, value);
        }

        public override void Instantiate(Pipeline pipeline, IList<InstanceEnvironment> instances) {
            Exporter exporter = null;
            foreach (var inputConfig in Inputs) {
                if (inputConfig is null) {
                    continue;
                }
                var remote = instances.Single(inst => inst.Configuration.Guid == inputConfig.Remote);
                var remoteDesc = ConfigurationManager.Description(remote.Configuration);
                var output = remoteDesc.Outputs.Single(o => o.Name == inputConfig.Output.PropertyName);
                dynamic emitter = null;
                switch (output) {
                    case VirtualPortDescription vOut:
                        emitter = remote.Instance;
                        break;
                    case StaticPortDescription sOut:
                        dynamic instProp = sOut.Property.GetValue(remote.Instance);
                        if (sOut.IsList) {
                            emitter = instProp[int.Parse(inputConfig.Output.Indexer)];
                        } else if (sOut.IsDictionary) {
                            emitter = instProp[inputConfig.Output.Indexer];
                        } else {
                            emitter = instProp;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                if (exporter is null) {
                    var storeEnv = instances.Where(e => e.Configuration.Guid == Exporter).SingleOrDefault();
                    if (storeEnv is null) {
                        throw new Exception($"Destination exporter of stream writer \"{Name}\" not set");
                    }
                    switch (storeEnv.Configuration) {
                        case LocalExporterConfiguration _:
                            exporter = (Exporter)storeEnv.Instance;
                            break;
                        case RemoteExporterConfiguration _:
                            exporter = ((RemoteExporter)storeEnv.Instance).Exporter;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                }
                Store.Write(emitter, StreamName, exporter, LargeMessage, DeliveryPolicy);
            }
        }

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Stream Writer",
            Description = "Write selected output as a stream to selected exporter.",
            ConfigurationType = typeof(StreamWriterImplConfiguration),
            Inputs = new [] {
                new VirtualPortDescription() {
                    VirtualName = "In",
                    PortType = PortType.Input,
                },
            },
        };

    }

    #endregion

    #region importer
    public class LocalImporterConfiguration : ImporterConfiguration {

        private string storeName = string.Empty;

        public string StoreName {
            get => storeName;
            set => SetProperty(ref storeName, value);
        }

        private string rootPath;

        public string RootPath {
            get => rootPath;
            set => SetProperty(ref rootPath, value);
        }

        public override object Instantiate(Pipeline pipeline) {
            //var exists = Store.Exists(StoreName, RootPath);
            return Store.Open(pipeline, StoreName, RootPath);
        }

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Local Importer",
            Description = "Get data from a local store.",
            ConfigurationType = typeof(LocalImporterConfiguration),
        };
    }

    public class RemoteImporterConfiguration : ImporterConfiguration {

        private string host = "localhost";

        public string Host {
            get => host;
            set => SetProperty(ref host, value);
        }

        private int port = 11411;

        public int Port {
            get => port;
            set => SetProperty(ref port, value);
        }

        private bool allowSequenceRestart = true;

        public bool AllowSequenceRestart {
            get => allowSequenceRestart;
            set => SetProperty(ref allowSequenceRestart, value);
        }

        public override object Instantiate(Pipeline pipeline) {
            var importer = new RemoteImporter(pipeline, ReplayDescriptor.ReplayAll.Interval, Host, Port, AllowSequenceRestart);
            var connected = importer.Connected.WaitOne(TimeSpan.FromSeconds(5));//otherwise the Importer field will be empty
            if (!connected) {
                throw new TimeoutException($"Connection with {Host}:{Port} timed out");
            }
            return importer;
        }

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Remote Importer",
            Description = "Get data from a remote host.",
            ConfigurationType = typeof(RemoteImporterConfiguration),
        };
    }
    #endregion

    #region stream reader
    public class StreamReaderImplConfiguration : StreamReaderConfiguration {

        private Guid importer;

        public Guid Importer {
            get => importer;
            set => SetProperty(ref importer, value);
        }

        private string streamName;

        public string StreamName {
            get => streamName;
            set => SetProperty(ref streamName, value);
        }

        public override object Instantiate(Pipeline pipeline, IList<InstanceEnvironment> importers, Type dataType) {
            var storeEnv = importers.Where(e => e.Configuration.Guid == Importer).SingleOrDefault();
            if (storeEnv is null) {
                throw new Exception($"Source importer of stream reader \"{Name}\" not set");
            }
            Importer importer = null;
            switch (storeEnv.Configuration) {
                case LocalImporterConfiguration _:
                    importer = (Importer)storeEnv.Instance;
                    break;
                case RemoteImporterConfiguration _:
                    importer = ((RemoteImporter)storeEnv.Instance).Importer;
                    break;
                default:
                    throw new InvalidOperationException();
            }
            //Note: importer.OpenDynamicStream returns IProducer<object> not IProducer<dynamic> as expected, which will cause error
            var method = importer
                .GetType()
                .GetMethod(nameof(Microsoft.Psi.Data.Importer.OpenStream))
                .MakeGenericMethod(dataType);
            var parameters = method.GetParameters();
            return method.Invoke(importer, new[] { StreamName, parameters[1].DefaultValue });
        }

        public static ComponentDescription Description => new ComponentDescription() {
            Name = "Stream Reader",
            Description = "Read selected stream from a selected importer.",
            ConfigurationType = typeof(StreamReaderImplConfiguration),
            Outputs = new [] {
                new VirtualPortDescription() {
                    VirtualName = "Out",
                    PortType = PortType.Output,
                },
            },
        };

    }
    #endregion
}
