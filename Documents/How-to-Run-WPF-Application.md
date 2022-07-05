# Introduction

A Windows desktop application that encapsulates all existing OpenSense components is provided.
The application is called OpenSense WPF Application.
With this application, users can try out existing sensing capabilities quickly.

This guide includes explanations of OpenSense concepts and App general instructions.
There are detailed samples available in other documents.

# Hardware & Software Requirements

OpenSense is designed to be multi-platform, while the application and some of the components can not.
The general requirements of the application are as follows:

+ Windows 10 or newer operating system.
+ X64 architecture CPU.

A minimum of limit of memory space is not listed, but we recommend a large memory capacity, because unprocessed data may be buffered in memory.
A high frequency CPU can help increase the throughput.

Some components require an Internet connection, typically cloud based services.
Cloud bases services will also require a valid account and some configuration efforts on their web sites priory to the usage.
Please refer to their respective documents to setup the cloud services and download access tokens provided by the cloud service provider.

Some OpenSense components requires dedicated sensor hardwares connected to the computer, such as an [Azure Kinect](https://azure.microsoft.com/en-us/services/kinect-dk).
Without a functioning hardware, those components will not work properly.

# Install

OpenSense WPF Application has 2 dependencies that is not included in the released packages.
These 2 dependencies need to be installed first.

The first dependency is __.NET 6`__.
You can download its latest installer from its [website](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
Please download and install an SDK installer, it contains functions of all kinds of Runtime installers.
A single kind of Runtime installer does not contain all the functions that may be used.

The second dependency is __Visual C++ Redistributable__.
Its latest installer is available at its [website](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist).
Please download and install it.

OpenSense WPF Application is published in ZIP files.
You can check for updates and download the latest package from OpenSense's Github [release page](https://github.com/intelligent-human-perception-laboratory/OpenSense/releases).
After you downloaded the package file, un-zip it to a folder.
The name of the executable file is `OpenSense.Wpf.exe`
Double click it will launch the application.

# Concepts

Here are some concepts that are important while using OpenSense.

## Microsoft Platform for Situated Intelligence (\psi)

OpenSense builds on top of [Microsoft Platform for Situated Intelligence](https://github.com/microsoft/psi).
Many concepts of OpenSense are inherited and extended from similar concepts in \psi.

## Component

A component is responsible for processing a certain type of data.
OpenSense includes some basic components for data acquisition, processing and saving.

It is very similar to nodes in node systems of creative applications, such as Unreal Engin and Blender, for helping group and reuse some logics. However, OpenSense components generally do heavier and more complex computations than nodes in those systems which contains light-weight logics.

Most components can be instantiated multiple times in a pipeline.
Their usage will be given in their description.

If you are a .NET developer, here are OpenSense's possibilities beyond the basic functions of the application.
OpenSense components are compatible to /psi components, so they can be extracted from OpenSense and used together with /psi.
New custom components can be added and used together with OpenSense.
The OpenSense WPF Application has an ability to detect new components and load them.

## Input & Output Port

A component has input and/or output port(s) for passing data.
A port is strictly typed, only data with a type matching the port's accepted type can be delivered through that port, so for and example an Audio Buffer type output port can not be connected to a Color Image type input port.
In OpenSense WPF Application, if you select an input port, only output ports with matching (or convertible) types will be listed for selection to connect with.

In addition to requiring a simple type, a port may also require an array of a type or an associative set of a type.
If a connection is made to or from this kind of complex port, an index or a key is required to designate a specific slot.
In OpenSense WPF Application, if you find a text box appear after an input or an output port, that is the place to fill the index or the key.
Indices should be integral numbers count started from 0, and keys should be texts.

A component may have multiple input ports.
Base on components internal designs, some ports may allow to be left hanging and un-connected, others may be required to be connected.
The usage of ports will be marked in their description.
Making a required input port hanging will block the component from processing data coming from other input ports.
The blocked data may be dropped or stuck in memory buffer, depends on the connections Delivery Policy setting.

A component may also have multiple output ports.
Output ports can be left not connected.
However, the computation may not be by-passed even if there are no consumers.
It is component designer's choice to enable the ability to by-pass or not if no output is consumed.

## Delivery Policy

For each connection, a delivery policy setting is required.
It helps control the behavior of the input buffer if the down-stream consumer component is slower than the up-stream producer component.

Typical settings are `Latest Message` and `Unlimited`.
`Latest Message` will drop excessive data and may cause distortions of results.
`Unlimited` will buffer all the un-processed input data in memory and may cause the computer run out of memory.
`Throttle` will slow down the up-stream component from producing data too fast.

Finer granularity of control such as setting a buffer size or a threshold time limit is possible.
But these are not currently supported in the OpenSense WPF Application.

## Pipeline

A pipeline is a container for components and their connections.
Components in a pipeline will be started and stopped in a whole.
If a pipeline is requested to stop, it will not stop immediately.
The pipeline will still run until buffered data are all consumed.
If a source component for data acquisition is implemented properly, it should not produce new data after a stop is requested.

OpenSense WPF Application will pop a dialog box after the pipeline comes to a complete stop.

Pipeline Editor will help create pipelines.
It can load and save pipelines.

Pipeline Executer is for loading and running pipelines.

If hardware related settings such as file paths and specific devices are set when creating a pipeline, when loading the saved pipeline on another machine, these settings need to be adjusted accordingly.

# Pipeline Editor

To load a pipeline, click `File` - `Open` then select a pipeline configuration file.
To save the current pipeline, click `File` - `Save as` then enter a filename for the configuration file.
If you edited a loaded pipeline, remember to use `Save as` function to save changes.
Changes are not automatically saved backed to its source file.
You can provide a name for the pipeline, this name will be shown in the Pipeline Executer.
A name can help identifying a pipeline if multiple pipelines are loaded into Pipeline Executer windows.
You can also set a default Delivery Policy for the pipeline.

To add a component, click the `Add` on the left.
A window listing all detected components will be shown.
A `Description` column will describe components' usages.
If you know the name of the component you want to add, type its name into the filter on the top of the window.
Double click the row of the component you want to add to instantiate that component once into the pipeline.
To remove a component, select that component then click the `Delete` button on the left.
If a component is removed, all its related connections will also be removed.

Once a component is selected on the left side of the editor window.
Component general options will be shown in the middle of the editor window.
Component specific options will be shown on the right side of the editor window.
We recommend renaming components to a suitable name, it will help quickly identifying them when making connections.

To make a connection between two components, first select the down-stream component on the left side of the editor window.
Then click the `Add` button in the middle to connect a input port.
Available input ports will be listed.
Double click the desired input port to include it into the collection of connections.
The name of the newly added input port will shown in red color in the middle of the editor window.
By selecting a input port, the ports accepted data type, connection's Delivery Policy and all valid output ports that it can be connected to will be listed next to it.
Select an output port to confirm the connection.
The red colored input port will change to black, indicating a valid connection exists for that port.

There is a shortcut loading the current pipeline in Pipeline Editor into Pipeline Executer.
Clicking `Execute` - `Execute` will open a Pipeline Executer window with the current pipeline loaded.

# Pipeline Executer

To load a pipeline, click `File` - `Open`.
Once the pipeline is loaded, you can run it by clicking `Execute` - `Run`.
Some components may have runtime UI shown in the bottom of the window, providing capabilities to modify their configurations and/or visualize their status.
To stop a pipeline, click `Execute` - `Stop` or simply close the window.
The window will freeze until the pipeline comes to a complete stop.
A dialog box will pop out indicating the pipeline is stopped.