/*============================================================================
                              AbyssChanSwitchWin.hpp
==============================================================================

  This declares class AbyssChanSwitchWin, which provides communication
  facilities for use with an AbyssServer object that uses Windows
  Winsock2 sockets.

============================================================================*/
#ifndef ABYSS_CHAN_SWITCH_WIN_HPP_INCLUDED
#define ABYSS_CHAN_SWITCH_WIN_HPP_INCLUDED

#include <xmlrpc-c/AbyssChanSwitch.hpp>

namespace xmlrpc_c {

#ifdef XMLRPC_BUILDING_ABYSSPP
#define XMLRPC_ABYSSPP_EXPORTED XMLRPC_DLLEXPORT
#else
#define XMLRPC_ABYSSPP_EXPORTED
#endif

class XMLRPC_ABYSSPP_EXPORTED AbyssChanSwitchWin : public AbyssChanSwitch {

public:
    AbyssChanSwitchWin(unsigned short const listenPortNum);
};

}  // namespace
#endif
