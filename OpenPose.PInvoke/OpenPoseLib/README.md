# Introduction

[OpenPose](https://github.com/CMU-Perceptual-Computing-Lab/openpose)

# Repository

With a customized image input worker-provider.

[fork](https://github.com/intelligent-human-perception-laboratory/openpose.git)

# Build Options

## CMake

Visual Studio 2017 (vs15).

Only x64 Target.

BUILD_EXAMPLES off

BUILD_UNITY_SUPPORT on

GPU_MODE (prefer CPU_ONLY?)

## Visual Studio

Unchanged.

Unicode not supported, use MultiByte char-set instead.

# How To Update OpenPoseLib

## Bin

For .dlls to put in `Common`, they are in `openpose/{build}/bin`, but you may not need all of them (such as CUDA libs).

You can delete all of them, than use CMake Configure again.

Copy DLLs to appropriate directories.

## Model

Use `getModels.bat` in OpenPose, it will download models into a `models` directory.

Copy that directory.