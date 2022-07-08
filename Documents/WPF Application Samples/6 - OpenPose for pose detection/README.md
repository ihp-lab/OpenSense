# OpenPose for pose detection

## Description

This sample contains a pipeline for demonstrating the usage of OpenPose component.

## How to use

+ Open the `sample.pipe.json` configuration file using a `Pipeline Editor` window.
+ Select the `Media Capture` instance and set a camera.
+ Run the pipeline.

## Steps to replicate

+ Add a `Media Capture` instance.
+ Add a `OpenPose` instance.
+ Add the `In` input and connect it to the `Out` output of the `Media Capture` instance.
+ Add a `OpenPose Visualizer` instance.
+ Add the `In` input and connect it to the `Out` output of the `OpenPose` instance.
+ Save the pipeline configuration.

## Tips

+ A CUDA enabled GPU is required.
+ This implementation is based on the official OpenPose Unity plug-in. For option information, please refer to the original source.
+ Only one OpenPose instance is allowed in each OpenSense process. This is the limitation of the backed, we may remove this limitation if there is a massive need. 
+ The data types inside `Datum` type for spacial calculations may be changed if there is a better solution.