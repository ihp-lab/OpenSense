# Architecture overview (draft)

OpenSense is build on top of Microsoft [Platform for Situated Intelligence](https://github.com/microsoft/psi).

Can be used as module or application.

## Use application

A WPF application, Windows only.

Quick design and run /psi pipeliens without writing codes.

Can save pipeline design to a file for later use.

Can detect new OpenSense components at runtime, application not need to be re-compiled.

## Use as module

Main architecture is targeting .net standard 2.0, so it is multi-platform.

Will publish Nuget packages to nuget.org once we switch the license to one of BSD licenses.

Added some /psi components that are not in /psi. They can be used alone. Will add more.

APIs for assemble a pipeline by reading a saved pipeline configuration file.

Modulized, select only components you want.

Implement interfaces to wrap a /psi component into a OpenSense component.

Still developing, APIs may have minor breaking changes.
