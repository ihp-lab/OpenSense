#ifndef ABYSS_REQHANDLER_XMLRPC_HPP_INCLUDED
#define ABYSS_REQHANDLER_XMLRPC_HPP_INCLUDED

#include <string>
#include <xmlrpc-c/AbyssServer.hpp>
#include <xmlrpc-c/registry.hpp>

namespace xmlrpc_c {

class abyssReqhandlerXmlrpc : public xmlrpc_c::AbyssServer::ReqHandler {
/*-----------------------------------------------------------------------------
   An object of this class is an Abyss request handler that you can
   use with an Abyss HTTP server object to make it an XML-RPC server.

   One way to use this is to make a derived class that represents a handler
   for a certain set of XML-RPC methods.  The derived class' constructor
   creates an appropriate registry to pass to this base class' constructor.
   
   The derived class can also have a 'handleUnreportableFailure' method to
   deal with RPC failures that the server is unable to report as RPC
   responses.

   Note: class abyssServer does not use this class; it uses a C Abyss server
   instead.
-----------------------------------------------------------------------------*/
public:
    abyssReqhandlerXmlrpc(xmlrpc_c::registryPtr const& registryP);

    void
    handleRequest(xmlrpc_c::AbyssServer::Session * const sessionP,
                  bool *                           const handledP);

private:
    xmlrpc_c::registryPtr const registryP;

    void
    abortRequest(AbyssServer::Session * const  sessionP,
                 bool                   const  responseStarted,
                 AbyssServer::Exception const& e);

    virtual void
    handleUnreportableFailure(AbyssServer::Exception const& e);
        // This method does whatever is appropriate when an RPC fails and the
        // server is unable to tell the caller.  Logging is typically all one
        // can do.  The default method, in the base class, does nothing.
};

}  // namespace

#endif
