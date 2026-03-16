HM HEVC Reference Software.
We're using a modified version of HM (https://github.com/KanaLab/HM) to fix high bit depth issues.

# Version

- **Upstream**: `https://vcgit.hhi.fraunhofer.de/jvet/HM` commit `dbe8d28d` (after HM 18.0)
- **Fork**: `https://github.com/KanaLab/HM` commit `5a9cfb2b`

# Fork Changes

- `0269a7d4` Fix integer overflow in dequantization shift for high bit depth
- `5a9cfb2b` Fix non-conforming delta weight range for high bit depth WP

# CMake Options

|Option|Value|Comment|
|-|-|-|
|BBuildEnv_USE_LIBRARY_NAME_POSTFIX|OFF|Simplify setup|
|CMAKE_BUILD_TYPE|Release|This option is not for Visual Studio|
|ENABLE_SEARCH_OPENSSL|ON|Can be added easily|
|ENABLE_TRACING|OFF|Avoid extra runtime cost|
|EXTENSION_360_VIDEO|OFF|Latest version not compatible with HM|
|HIGH_BITDEPTH|ON|16-bit support is required|
|SET_ENABLE_TRACING|OFF|Not needed|

# Artifacts

Only projects under `lib` will be built, `app` projects will not be used.
Other build types like `MinSizeRel` and `RelWithDebInfo` exist but are not included here.
