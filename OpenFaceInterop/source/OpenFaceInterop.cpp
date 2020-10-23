// Allows to overcome boost name clash stuff with C++ CLI
#ifdef __cplusplus_cli
#define generic __identifier(generic)
#endif

// This is the main DLL file.
#include "FaceAnalyserInterop.h"
#include "FaceDetectorInterop.h"
#include "GazeAnalyserInterop.h"
#include "ImageReaderInterop.h"
#include "LandmarkDetectorInterop.h"
#include "MethodsInterop.h"
#include "RecorderInterop.h"
#include "SequenceReaderInterop.h"
#include "VisualizerInterop.h"

#ifdef __cplusplus_cli
#undef generic
#endif