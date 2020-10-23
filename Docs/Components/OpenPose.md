# Function

A wrapper of [OpenPose](https://github.com/CMU-Perceptual-Computing-Lab/openpose), which can detect people in video frames.

# Hardware

A CUDA enabled GPU is required.

# Class

`OpenSense.Components.OpenPoseComponent`

# Input

`Microsoft.Psi.Shared<Microsoft.Psi.Imaging.Image>`

# Output

The returned value is `OpenPose.OpenPoseDatum`, which is a wrapper of OpenPose's result for one input frame.

Fields are available if the corresponding functions are enabled in configuration.

# Configuration

The constructor of OpenPose component require a instance of `OpenPosePInvoke.Configuration.StaticConfiguration` to setup the OpenPose.

The StaticConfiguration instance consists detail configurations.

+ Pose
+ Hand
+ Face
+ Extra
+ Input
+ Output
+ Gui
+ Debugging

They all come from the original OpenPose configuration.
The default constructor returns a default configuration, this default configuration is mainly based on the OpenPose Unity wrapper's default configuration. However, the input and output configurations are redirected to adapt OpenSense.
For values in the default configuration, please refer to the [OpenPose Unity plugin repository](https://github.com/CMU-Perceptual-Computing-Lab/openpose_unity_plugin/blob/master/OpenPosePlugin/Assets/OpenPose/Modules/Scripts/OPWrapper.cs).

The size of neural network in the default configuration may not fit well into a regular GPU's graphic memory. Thus, it may be necessary to adjust the size before passing the configuration into component's constructor.

The default `Input` configuration sets the frame source to be the OpenSense's OpenPose interface. Other input sources such as webcams can be used directly, while it is recommended to use this OpenPose component as a processing unit rather than a data source.