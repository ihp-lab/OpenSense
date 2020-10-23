#ifndef GIRERR_HPP_INCLUDED
#define GIRERR_HPP_INCLUDED

#include <string>
#include <exception>

#include <xmlrpc-c/c_util.h>

/*
XMLRPC_LIBUTILPP_EXPORTED marks a symbol in this file that is exported from
libxmlrpc_util++.

XMLRPC_BUILDING_LIBUTILPP says this compilation is part of libxmlrpc_util++, as
opposed to something that _uses_ libxmlrpc_util++.
*/
#ifdef XMLRPC_BUILDING_LIBUTILPP
#define XMLRPC_LIBUTILPP_EXPORTED XMLRPC_DLLEXPORT
#else
#define XMLRPC_LIBUTILPP_EXPORTED
#endif

#define HAVE_GIRERR_ERROR

namespace girerr {

class XMLRPC_LIBUTILPP_EXPORTED error : public std::exception {
public:
    error(std::string const& what_arg) : _what(what_arg) {}

    ~error() throw() {}

    virtual const char *
    what() const throw() { return this->_what.c_str(); };

private:
    std::string _what;
};

// throwf() always throws a girerr::error .

XMLRPC_LIBUTILPP_EXPORTED
void
throwf(const char * const format, ...)
  XMLRPC_PRINTF_ATTR(1,2)
  XMLRPC_NORETURN_ATTR;

} // namespace

#endif
