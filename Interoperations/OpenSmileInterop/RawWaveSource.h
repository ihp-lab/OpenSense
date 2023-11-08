


/*  openSMILE component:

example dataSource
writes data to data memory...

*/


#pragma once

//#include <mutex>
#include <vector>
#include <core/smileCommon.hpp>
#include <core/dataSource.hpp>

#define COMPONENT_DESCRIPTION_CRAWWAVESOURCE "This is an example of a cDataSource descendant. It writes random data to the data memory. This component is intended as a template for developers."
#define COMPONENT_NAME_CRAWWAVESOURCE "cRawWaveSource"

#undef class
class cRawWaveSource : public cDataSource {
private:
	//std::mutex bufferLock = std::mutex();
	std::vector<uint8_t> buffer = std::vector<uint8_t>();
	bool cRawWaveSource::readData();
protected:
	
	const char *outFieldName;
	bool monoMixdown = true;
	long sampleRate;
	int channels;
	int sampleSize;
	

    SMILECOMPONENT_STATIC_DECL_PR
    
    virtual void fetchConfig();
    //virtual int myConfigureInstance();
    //virtual int myFinaliseInstance();
    virtual int myTick(long long t);

    virtual int configureWriter(sDmLevelConfig &c);
    virtual int setupNewNames(long nEl);

public:
	void feedData(const uint8_t * transBuffer, const int frameBufferLength);

    SMILECOMPONENT_STATIC_DECL
    
    cRawWaveSource(const char *_name);

};