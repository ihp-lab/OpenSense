openSMILE by audEERING GmbH.

# Version

- **Upstream**: `https://github.com/audeering/opensmile` tag `v3.0.2` commit `99e8db3`

The upstream did not bump the version string for this release, so the binaries report themselves as `3.0.1`.

# CMake Options

|Option|Value|Comment|
|-|-|-|
|BUILD_FLAGS|`-DBUILD_LIBSVM -DBUILD_RNN -DBUILD_SVMSMO`|Same component set as the official binaries|
|STATIC_LINK|ON|Linked into OpenSmileInterop|
|WITH_FFMPEG|OFF|Audio is fed by the pipeline|
|WITH_OPENCV|OFF|Video features are not used|
|WITH_PORTAUDIO|OFF|Audio is fed by the pipeline|

# Artifacts

Only the `opensmile` target is built, in `Debug` and `Release`.
The `newmat` objects are packed into `opensmile.lib`, but their debug information is in the separate `newmat.pdb`.

`include` is `src/include` of the source tree, plus `core/git_version.hpp`, which CMake generates into the build directory.
