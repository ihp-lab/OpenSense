#include "ComponentManager.h"
#include "ComponentList.h"

cInteropComponentManager::cInteropComponentManager(cConfigManager *_confman) : cComponentManager(_confman, componentlist) {
	//register interop components
	registerComponentTypes(interopComponentList);
}