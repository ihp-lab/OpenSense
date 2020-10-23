/*============================================================================
                              AbyssServer.hpp
==============================================================================
  This declares class AbyssServer, which provides facilities for running an
  Abyss server, which is a simple HTTP server
============================================================================*/
#ifndef ABYSS_SERVER_HPP_INCLUDED
#define ABYSS_SERVER_HPP_INCLUDED

#include <stdexcept>

#include <vector>
#include <string>
#include <map>
#if defined(__GNUC__) && __GNUC__ < 3
#include <iostream>
#else
#include <ostream>
#endif
#include <xmlrpc-c/abyss.h>
#include <xmlrpc-c/AbyssChanSwitch.hpp>

namespace xmlrpc_c {

#ifdef XMLRPC_BUILDING_ABYSSPP
#define XMLRPC_ABYSSPP_EXPORTED XMLRPC_DLLEXPORT
#else
#define XMLRPC_ABYSSPP_EXPORTED
#endif

class XMLRPC_ABYSSPP_EXPORTED AbyssServer {
/*----------------------------------------------------------------------------
   An object of this class represents an Abyss HTTP server.

   The class does not provide any specific HTTP service, such as web service
   from files or XML-RPC.  Rather, it provides a basic framework that handles
   the details of the HTTP protocol and the user provides request handlers
   that do the real work.  For example, the user can provide an XML-RPC
   request handler and then the server is an XML-RPC server.
-----------------------------------------------------------------------------*/
public:

    class XMLRPC_ABYSSPP_EXPORTED Exception : public std::runtime_error {
    public:
        Exception(unsigned short const  httpStatusCode,
                  std::string    const& explanation);

        unsigned short
        httpStatusCode() const;

    private:
        unsigned short _httpStatusCode;
    };

    class XMLRPC_ABYSSPP_EXPORTED Session {
    public:
        class Impl;

        Session(TSession * const cSessionP);

        ~Session();

        enum Method {
            METHOD_UNKNOWN,
            METHOD_GET,
            METHOD_PUT,
            METHOD_HEAD,
            METHOD_POST,
            METHOD_DELETE,
            METHOD_TRACE,
            METHOD_OPTIONS
        };

        Method
        method() const;

        std::string const
        requestLine() const;

        std::string const
        uriPathName() const;

        std::vector<std::string> const
        uriPathNameSegment() const;

        bool
        uriHasQuery() const;

        std::string const
        uriQuery() const;

        std::map<std::string, std::string> const
        formInput() const;

        bool
        hasHost() const;

        std::string const
        host() const;

        bool
        hasFrom() const;

        std::string const
        from() const;

        bool
        hasUseragent() const;

        std::string const
        useragent() const;

        bool
        hasReferer() const;

        std::string const
        referer() const;

        bool
        userIsAuthenticated() const;

        std::string const
        user() const;

        unsigned short
        port() const;

        bool
        keepalive() const;

        bool
        hasContentLength() const;

        size_t
        contentLength() const;

        std::string const
        body();

        void
        readRequestBody(unsigned char * const buffer,
                        size_t          const size);

        void
        readSomeRequestBody(size_t          const max,
                            unsigned char * const buffer,
                            bool *          const eofP,
                            size_t *        const byteCtP);

        void
        getHeaderField(std::string   const& fieldName,
                       bool *        const  isPresentP,
                       std::string * const  valueP) const;

        std::string const
        headerFieldValue(std::string const& fieldName) const;

        void
        setRespStatus(unsigned short const statusCode);

        void
        setRespContentType(std::string const& contentType);

        void
        setRespContentLength(uint64_t const contentLength);

        void
        startWriteResponse();

        void
        endWriteResponse();

        void
        writeResponseBody(const unsigned char * const data,
                          size_t                const size);

        void
        writeResponse(std::string const& body);

        void
        sendErrorResponse(std::string const& explanation);

        void
        sendErrorResponse(Exception const& e);

    private:

        Impl * const implP;
    };

    class XMLRPC_ABYSSPP_EXPORTED ReqHandler {
    /*------------------------------------------------------------------------
      An object of this class handles HTTP requests, which essentially means
      it provides responses to them.  An AbyssServer object calls methods of
      objects of this class to execute HTTP requests.

      The object also decides and reports whether a particular request is
      appropriate for this object to handle.  An AbyssServer object may have
      multiple ReqHandler objects on which it can call, each specializing in
      different kinds of requests.

      This is an abstract base class.  A derived class knows how to handle,
      for example, HTTP requests that are XML-RPC RPCs.
    -------------------------------------------------------------------------*/
    public:
        virtual
        ~ReqHandler();

        virtual void
        handleRequest(Session * const sessionP,
                      bool *    const handledP) = 0;

        virtual size_t
        handleReqStackSize() const;

        virtual void
        terminate();
    };

    AbyssServer(AbyssChanSwitch * const chanSwitchP);

    ~AbyssServer();

    void
    addRequestHandler(ReqHandler * const handlerP);

    void
    init();
    
    void
    run();

    void
    runOnce();

    void
    terminate();

    void
    resetTerminate();

private:
    TServer cServer;
};

}  // namespace

XMLRPC_ABYSSPP_EXPORTED
std::ostream& operator<<(std::ostream                                & out,
                         xmlrpc_c::AbyssServer::Session::Method const& method);

#endif
