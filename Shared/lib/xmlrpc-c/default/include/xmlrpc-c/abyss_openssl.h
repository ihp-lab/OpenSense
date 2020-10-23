#ifndef ABYSS_OPENSSL_H_INCLUDED
#define ABYSS_OPENSSL_H_INCLUDED
/*=============================================================================
                               abyss_openssl.h
===============================================================================
  Declarations to be used with an Abyss server that can server HTTPS URLs
  via OpenSSL.

  Note that there is no equivalent of this file for the original two Abyss
  channel types, "Unix" and "Windows".  Those are built into
  <xmlrpc-c/abyss.h>, for historical reasons.
=============================================================================*/
#ifdef __cplusplus
extern "C" {
#endif

#include <sys/socket.h>
#include <xmlrpc-c/abyss.h>
#include <openssl/ssl.h>

struct abyss_openSsl_chaninfo {
    size_t peerAddrLen;
        /* Length of 'peerAddr' (which is effectively polymorphic, so could
           have any of various lengths depending upon the type of socket
           address
        */
    struct sockaddr peerAddr;

    SSL * sslP;
        /* The handle of the OpenSSL connection object underlying this channel.
           
           You can use this to get all sort of neat information about the
           connection, such as the verified certification the client
           presented, using the OpenSSL library.  (For example, to find out
           the authenticated name of the client, use
           SSL_get_peer_certificate(), and use X509_get_subject name() with
           the result of that).

           This is kind of a modularity violation, which we don't mind because
           it is so easy and flexible.  But note that it is the Abyss design
           intent that you use the SSL object _only_ to get information about
           the channel.
        */
};

void
ChanSwitchOpenSslCreate(int                     const protocolFamily,
                        const struct sockaddr * const sockAddrP,
                        socklen_t               const sockAddrLen,
                        SSL_CTX *               const sslCtxP,
                        TChanSwitch **          const chanSwitchPP,
                        const char **           const errorP);

void
ChanSwitchOpenSslCreateIpV4Port(unsigned short const portNumber,
                                SSL_CTX *      const sslCtxP,
                                TChanSwitch ** const chanSwitchPP,
                                const char **  const errorP);

void
ChanSwitchOpenSslCreateIpV6Port(unsigned short const portNumber,
                                SSL_CTX *      const sslCtxP,
                                TChanSwitch ** const chanSwitchPP,
                                const char **  const errorP);

void
ChanSwitchOpenSslCreateFd(int            const fd,
                          SSL_CTX *      const sslCtxP,
                          TChanSwitch ** const chanSwitchPP,
                          const char **  const errorP);

void
ChannelOpenSslCreateSsl(SSL *                            const sslP,
                        TChannel **                      const channelPP,
                        struct abyss_openSsl_chaninfo ** const channelInfoPP,
                        const char **                    const errorP);

#ifdef __cplusplus
}
#endif

#endif
