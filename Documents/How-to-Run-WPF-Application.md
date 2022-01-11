# Introduction

Due to some limitations, there are still several steps before you can run the OpenSense WPF application.

# Download

Binary releases are available on Github [release page](https://github.com/intelligent-human-perception-laboratory/OpenSense/releases).

# Install Dependencies

## .Net 6 Runtime (x64)

The runtime can be downloaded from [Microsoft's official site](https://dotnet.microsoft.com).

## Kinect V2 SDK

If you do not want to use Kinect V2 (an old device), delete `OpenSense.Component.Psi.Kinect.Windows.dll` and `OpenSense.Wpf.Component.Psi.Kinect.Windows.dll` before you run it.
This is because these DLLs depend on the Kinect V2 SDK, and if that SDK are not found, they loading these 2 files will give an error and make the application crash.
If we delete them, the application will not try to load them, then it will not try to find the SDK, so there will be no error.

Or you can install [Kinect for Windows SDK 2.0](https://www.microsoft.com/en-us/download/details.aspx?id=44561), then run the application.

# Modify CAS Policy

Since the WPF application detects available components and loads them dynamically, the default security policy will stop this load behavior, and you will get an error.

The solution is that modify a file called `OpenSense.Wpf.exe.config` next to the application EXE file. It is a plain text file.
Add a line `<loadFromRemoteSources enabled="true"/>` under the line `<runtime>` of `<configuration>`. Then you may run the EXE file.

Another solution is building the project yourself, so that those DLLs will not be treated as dangerous files.

# Run

Run `OpenSense.Wpf.exe`.
There are tutorial documents about how to use this application.