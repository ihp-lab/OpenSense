# Version

- **Repo**: `https://vcgit.hhi.fraunhofer.de/jvet/HM`
- **Commit**: `ae4c8d6d67572e929610a205eb574bd8dc00c043` after HM 18.0.

# Changes

To resolve build errors (warnings treated as errors), the source code was modified.

File: `source/Lib/TLibCommon/TComTrQuant.cpp`

```cpp
// Line 1364 before
const Intermediate_Int iAdd = 1 << (rightShift - 1);
// Line 1364 after
const Intermediate_Int iAdd = Intermediate_Int(1) << (rightShift - 1);

// Line 1401 before
const Intermediate_Int iAdd = 1 << (rightShift - 1);
// Line 1401 after
const Intermediate_Int iAdd = Intermediate_Int(1) << (rightShift - 1);

// Line 2041 before
const TCoeff offset = 1 << (iTransformShift - 1);
// Line 2041 after
const TCoeff offset = TCoeff(1) << (iTransformShift - 1);

// Line 3417 before
const Intermediate_Int iAdd      = 1 << (rightShift - 1);
// Line 3417 after
const Intermediate_Int iAdd      = Intermediate_Int(1) << (rightShift - 1);

// Line 3443 before
const Intermediate_Int iAdd      = 1 << (rightShift - 1);
// Line 3443 after
const Intermediate_Int iAdd      = Intermediate_Int(1) << (rightShift - 1);
```

# CMake Options

|Option|Value|Comment|
|-|-|-|
|BBuildEnv_USE_LIBRARY_NAME_POSTFIX|OFF|Simplify setup|
|CMAKE_BUILD_TYPE|Release|This option is not for Visual Studio|
|ENABLE_SEARCH_OPENSSL|ON|Can be added easily|
|ENABLE_TRACING|OFF|Avoid extra runtime cost|
|EXTENSION_360_VIDEO|OFF|Latest version not compatible with HM|
|HIGH_BITDEPTH|ON|16-bit support is required|
|SET_ENABLE_TRACING|OFF|Don't know what is this|

# Artifacts

Only projects under `lib` will be built, `app` projects will not be used.
Other build types like `MinSizeRel` and `RelWithDebInfo` exist but are not included here.