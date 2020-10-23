/*============================================================================
                            AbyssChanSwitchOpenSsl.hpp
==============================================================================

  This declares class AbyssChanSwitchOpenSsl, which provides communication
  facilities for use with an AbyssServer object that uses Unix sockets.

============================================================================*/
#ifndef ABYSS_CHAN_SWITCH_OPEN_SSL_HPP_INCLUDED
#define ABYSS_CHAN_SWITCH_OPEN_SSL_HPP_INCLUDED

#include <sys/socket.h>
#include <openssl/ssl.h>

#include <xmlrpc-c/AbyssChanSwitch.hpp>

namespace xmlrpc_c {

class AbyssChanSwitchOpenSsl : public AbyssChanSwitch {

public:

    AbyssChanSwitchOpenSsl(
        int                     const protocolFamily,
        const struct sockaddr * const sockAddrP,
        socklen_t               const sockAddrLen,
        SSL_CTX *               const sslCtxP);

    AbyssChanSwitchOpenSsl(
        unsigned short const listenPortNum,
        SSL_CTX *      const sslCtxP);
};

}  // namespace
#endif
