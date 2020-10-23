using System.Collections.Generic;
using Microsoft.Psi;
using OpenSense.Components;
using OpenSense.Components.Biopac;
using OpenSense.Components.Display;
using OpenSense.GazeToDisplayConverter;
using OpenSense.Utilities.DataWriter;

namespace OpenSense.PipelineBuilder.Configurations {

    #region Producer
    public class BiopacConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new Biopac(pipeline) ;

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Biopac).FullName,
            Description = "",
            ConfigurationType = typeof(BiopacConfiguration),
            Outputs = new[] {
                new StaticPortDescription() {
                    Property = typeof(Biopac).GetProperty(nameof(Biopac.Out)),
                }
            },
        };
    }
    #endregion

    #region Component
    public class FlipColorVideoConfiguration : ComponentConfiguration {

        private bool flipHorizontal = false;

        public bool FlipHorizontal {
            get => flipHorizontal;
            set => SetProperty(ref flipHorizontal, value);
        }

        private bool flipVertical = false;

        public bool FlipVertical {
            get => flipVertical;
            set => SetProperty(ref flipVertical, value);
        }

        public override object Instantiate(Pipeline pipeline) => new FlipColorVideo(pipeline) { FlipHorizontal = FlipHorizontal, FlipVertical = FlipVertical };

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(FlipColorVideo).FullName,
            Description = "",
            ConfigurationType = typeof(FlipColorVideoConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(FlipColorVideo).GetProperty(nameof(FlipColorVideo.In)),
                }
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(FlipColorVideo).GetProperty(nameof(FlipColorVideo.Out)),
                }
            },
        };
    }

    public class GazeToDisplayConfiguration : ComponentConfiguration {

        private string converterFilename;

        public string ConverterFilename {
            get => converterFilename;
            set => SetProperty(ref converterFilename, value);
        }

        public override object Instantiate(Pipeline pipeline) => new GazeToDisplay(pipeline) { Converter = string.IsNullOrEmpty(ConverterFilename) ? null : GazeToDisplayConverterHelper.Load(ConverterFilename) };

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(GazeToDisplay).FullName,
            Description = "",
            ConfigurationType = typeof(GazeToDisplayConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(GazeToDisplay).GetProperty(nameof(GazeToDisplay.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(GazeToDisplay).GetProperty(nameof(GazeToDisplay.Out)),
                },
            },
        };
    }

    public class OpenPoseConfiguration : ComponentConfiguration {

        private OpenPosePInvoke.Configuration.StaticConfiguration raw = new OpenPosePInvoke.Configuration.StaticConfiguration();

        public OpenPosePInvoke.Configuration.StaticConfiguration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new Components.OpenPose.OpenPose(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Components.OpenPose.OpenPose).FullName,
            Description = "",
            ConfigurationType = typeof(OpenPoseConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenPose.OpenPose).GetProperty(nameof(Components.OpenPose.OpenPose.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenPose.OpenPose).GetProperty(nameof(Components.OpenPose.OpenPose.Out)),
                },
            },
        };
    }

    public class OpenFaceConfiguration : ComponentConfiguration {

        private float cameraCalibFx = 500;

        public float CameraCalibFx {
            get => cameraCalibFx;
            set => SetProperty(ref cameraCalibFx, value);
        }

        private float cameraCalibFy = 500;

        public float CameraCalibFy {
            get => cameraCalibFy;
            set => SetProperty(ref cameraCalibFy, value);
        }

        private float cameraCalibCx = 640 / 2f;

        public float CameraCalibCx {
            get => cameraCalibCx;
            set => SetProperty(ref cameraCalibCx, value);
        }

        private float cameraCalibCy = 480 / 2f;

        public float CameraCalibCy {
            get => cameraCalibCy;
            set => SetProperty(ref cameraCalibCy, value);
        }

        private DataWriterConfiguration dataWriterConfiguration = new DataWriterConfiguration();

        public DataWriterConfiguration DataWriterConfiguration {
            get => dataWriterConfiguration;
            set => SetProperty(ref dataWriterConfiguration, value);
        }

        public override object Instantiate(Pipeline pipeline) => new Components.OpenFace.OpenFace(pipeline) { 
            CameraCalibFx = CameraCalibFx,
            CameraCalibFy = CameraCalibFy,
            CameraCalibCx = CameraCalibCx,
            CameraCalibCy = CameraCalibCy,
            DataWriter = new CsvWriter(pipeline, DataWriterConfiguration),
        };

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Components.OpenFace.OpenFace).FullName,
            Description = "",
            ConfigurationType = typeof(OpenFaceConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenFace.OpenFace).GetProperty(nameof(Components.OpenFace.OpenFace.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenFace.OpenFace).GetProperty(nameof(Components.OpenFace.OpenFace.HeadPoseOut)),
                },
                new StaticPortDescription() {
                    Property = typeof(Components.OpenFace.OpenFace).GetProperty(nameof(Components.OpenFace.OpenFace.GazeOut)),
                },
                new StaticPortDescription() {
                    Property = typeof(Components.OpenFace.OpenFace).GetProperty(nameof(Components.OpenFace.OpenFace.HeadPoseAndGazeOut)),
                },
            },
        };
    }

    public class OpenSmileConfiguration : ComponentConfiguration {

        private OpenSmileInterop.Configuration raw = new OpenSmileInterop.Configuration() { UseConfigurationFile = true };

        public OpenSmileInterop.Configuration Raw {
            get => raw;
            set => SetProperty(ref raw, value);
        }

        public override object Instantiate(Pipeline pipeline) => new Components.OpenSmile.OpenSmile(pipeline, Raw);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Components.OpenSmile.OpenSmile).FullName,
            Description = "",
            ConfigurationType = typeof(OpenSmileConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenSmile.OpenSmile).GetProperty(nameof(Components.OpenSmile.OpenSmile.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.OpenSmile.OpenSmile).GetProperty(nameof(Components.OpenSmile.OpenSmile.Out)),
                },
            },
        };
    }

    public class HeadGestureDetectorConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new Components.Onnx.HeadGestureDetector(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Components.Onnx.HeadGestureDetector).FullName,
            Description = "Detect head gesture from given head pose information",
            ConfigurationType = typeof(HeadGestureDetectorConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.Onnx.HeadGestureDetector).GetProperty(nameof(Components.Onnx.HeadGestureDetector.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.Onnx.HeadGestureDetector).GetProperty(nameof(Components.Onnx.HeadGestureDetector.Out)),
                },
            },
        };
    }

    public class EmotionDetectorConfiguration : ComponentConfiguration {

        private DataWriterConfiguration dataWriterConfiguration = new DataWriterConfiguration();

        public DataWriterConfiguration DataWriterConfiguration {
            get => dataWriterConfiguration;
            set => SetProperty(ref dataWriterConfiguration, value);
        }

        public override object Instantiate(Pipeline pipeline) => new Components.Onnx.EmotionDetector(pipeline) { 
            DataWriter = new CsvWriter(pipeline, DataWriterConfiguration) 
        };

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(Components.Onnx.EmotionDetector).FullName,
            Description = "",
            ConfigurationType = typeof(EmotionDetectorConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.Onnx.EmotionDetector).GetProperty(nameof(Components.Onnx.EmotionDetector.ImageIn)),
                },
                new StaticPortDescription() {
                    Property = typeof(Components.Onnx.EmotionDetector).GetProperty(nameof(Components.Onnx.EmotionDetector.HeadPoseIn)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(Components.Onnx.EmotionDetector).GetProperty(nameof(Components.Onnx.EmotionDetector.Out)),
                },
            },
        };
    }

    #endregion


    #region Visualizor
    public class BooleanVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new BooleanVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(BooleanVisualizer).FullName,
            Description = "Visualize boolean",
            ConfigurationType = typeof(BooleanVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(BooleanVisualizer).GetProperty(nameof(BooleanVisualizer.In)),
                },
            },
        };
    }

    public class ColorVideoVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new ColorVideoVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(ColorVideoVisualizer).FullName,
            Description = "Visualize RGB video",
            ConfigurationType = typeof(ColorVideoVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(ColorVideoVisualizer).GetProperty(nameof(ColorVideoVisualizer.In)),
                },
            },
        };
    }

    public class DepthVideoVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new DepthVideoVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(DepthVideoVisualizer).FullName,
            Description = "Visualize depth video",
            ConfigurationType = typeof(DepthVideoVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(DepthVideoVisualizer).GetProperty(nameof(DepthVideoVisualizer.In)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(DepthVideoVisualizer).GetProperty(nameof(DepthVideoVisualizer.Out)),
                },
            },
        };
    }

    public class AudioVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new AudioVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AudioVisualizer).FullName,
            Description = "Visualize audio signal",
            ConfigurationType = typeof(AudioVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AudioVisualizer).GetProperty(nameof(AudioVisualizer.In)),
                },
            },
        };
    }

    public class BiopacVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new BiopacVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(BiopacVisualizer).FullName,
            Description = "Visualize Biopac signal",
            ConfigurationType = typeof(BiopacVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(BiopacVisualizer).GetProperty(nameof(BiopacVisualizer.In)),
                },
            },
        };
    }

    public class OpenFaceVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new OpenFaceVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(OpenFaceVisualizer).FullName,
            Description = "Visualize OpenFace's output",
            ConfigurationType = typeof(OpenFaceVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(OpenFaceVisualizer).GetProperty(nameof(OpenFaceVisualizer.DataIn)),
                },
                new StaticPortDescription() {
                    Property = typeof(OpenFaceVisualizer).GetProperty(nameof(OpenFaceVisualizer.ImageIn)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(OpenFaceVisualizer).GetProperty(nameof(OpenFaceVisualizer.Out)),
                },
            },
        };
    }

    public class OpenPoseVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new OpenPoseVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(OpenPoseVisualizer).FullName,
            Description = "Visualize OpenPose's output",
            ConfigurationType = typeof(OpenPoseVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(OpenPoseVisualizer).GetProperty(nameof(OpenPoseVisualizer.DataIn)),
                },
                new StaticPortDescription() {
                    Property = typeof(OpenPoseVisualizer).GetProperty(nameof(OpenPoseVisualizer.ImageIn)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(OpenPoseVisualizer).GetProperty(nameof(OpenPoseVisualizer.Out)),
                },
            },
        };
    }

    public class AzureKinectBodyTrackerVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new AzureKinectBodyTrackerVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(AzureKinectBodyTrackerVisualizer).FullName,
            Description = "Visualize Azure Kinect body tracker's output",
            ConfigurationType = typeof(AzureKinectBodyTrackerVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTrackerVisualizer).GetProperty(nameof(AzureKinectBodyTrackerVisualizer.BodiesIn)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTrackerVisualizer).GetProperty(nameof(AzureKinectBodyTrackerVisualizer.CalibrationIn)),
                },
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTrackerVisualizer).GetProperty(nameof(AzureKinectBodyTrackerVisualizer.ColorImageIn)),
                },
            },
            Outputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(AzureKinectBodyTrackerVisualizer).GetProperty(nameof(AzureKinectBodyTrackerVisualizer.Out)),
                },
            },
        };
    }


    public class StreamingSpeechRecognitionVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new StreamingSpeechRecognitionVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(StreamingSpeechRecognitionVisualizer).FullName,
            Description = "Visualize speech recognition output",
            ConfigurationType = typeof(StreamingSpeechRecognitionVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(StreamingSpeechRecognitionVisualizer).GetProperty(nameof(StreamingSpeechRecognitionVisualizer.In)),
                },
            },
        };
    }

    public class OpenSmileVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new OpenSmileVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(OpenSmileVisualizer).FullName,
            Description = "Visualize openSMILE output",
            ConfigurationType = typeof(OpenSmileVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(OpenSmileVisualizer).GetProperty(nameof(OpenSmileVisualizer.In)),
                },
            },
        };
    }

    public class HeadGestureVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new HeadGestureVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(HeadGestureVisualizer).FullName,
            Description = "Visualize head gesture",
            ConfigurationType = typeof(HeadGestureVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(HeadGestureVisualizer).GetProperty(nameof(HeadGestureVisualizer.In)),
                },
            },
        };
    }

    public class GazeToDisplayVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new GazeToDisplayVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(GazeToDisplayVisualizer).FullName,
            Description = "",
            ConfigurationType = typeof(GazeToDisplayVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(GazeToDisplayVisualizer).GetProperty(nameof(GazeToDisplayVisualizer.In)),
                },
            },
        };
    }

    public class EmotionVisualizerConfiguration : ComponentConfiguration {

        public override object Instantiate(Pipeline pipeline) => new EmotionVisualizer(pipeline);

        public static ComponentDescription Description => new ComponentDescription() {
            Name = typeof(EmotionVisualizer).FullName,
            Description = "",
            ConfigurationType = typeof(EmotionVisualizerConfiguration),
            Inputs = new [] {
                new StaticPortDescription() {
                    Property = typeof(EmotionVisualizer).GetProperty(nameof(EmotionVisualizer.In)),
                },
            },
        };
    }
    #endregion
}
