#pragma once
#include "ComponentManager.h"

#include "WaveSourceClone.h"
#include "RawWaveSource.h"
#include "RawDataSink.h"
const registerFunction interopComponentList[] = {
	cWaveSourceClone::registerComponent,
	cRawWaveSource::registerComponent,
	cRawDataSink::registerComponent,

//------------------------------------------------------
	NULL   // the last element must always be NULL !
};