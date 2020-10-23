/*============================================================================
                              AbyssChanSwitchUnix.hpp
==============================================================================

  This declares class AbyssChanSwitchUnix, which provides communication
  facilities for use with an AbyssServer object that uses Unix sockets.

============================================================================*/
#ifndef ABYSS_CHAN_SWITCH_UNIX_HPP_INCLUDED
#define ABYSS_CHAN_SWITCH_UNIX_HPP_INCLUDED

#include <xmlrpc-c/AbyssChanSwitch.hpp>

namespace xmlrpc_c {

class AbyssChanSwitchUnix : public AbyssChanSwitch {

public:
    AbyssChanSwitchUnix(unsigned short const listenPortNum);
};

}  // namespace
#endif
