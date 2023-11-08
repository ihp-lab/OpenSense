


#pragma once
#include "gcroot.h"
#include <core/smileCommon.hpp>
#include <core/dataSink.hpp>
#include "DataMemory.h"

#define COMPONENT_DESCRIPTION_CRAWDATASINK "This is an example of a cDataSink descendant. It reads data from the data memory and prints it to the console. This component is intended as a template for developers."
#define COMPONENT_NAME_CRAWDATASINK "cRawDataSink"

#undef class
namespace OpenSmileInterop {
	public delegate void RawDataHandler(Vector^);
}

using namespace OpenSmileInterop;

class cRawDataSink : public cDataSink {
private:
	//output data <lag> frames behind
	//current fixed to 0
	const int lag = 0;
	bool fieldInfo;
protected:
	SMILECOMPONENT_STATIC_DECL_PR

	virtual void fetchConfig();
	//virtual int myConfigureInstance();
	//virtual int myFinaliseInstance();
	virtual int myTick(long long t);

public:
	gcroot<RawDataHandler^> rawDataHandlers = nullptr;

	SMILECOMPONENT_STATIC_DECL

	cRawDataSink(const char *_name);
};
