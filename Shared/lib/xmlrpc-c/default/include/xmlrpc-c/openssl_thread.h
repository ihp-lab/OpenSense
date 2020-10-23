#ifndef XMLRPC_OPENSS_THREAD_H_INCLUDED
#define XMLRPC_OPENSS_THREAD_H_INCLUDED

void
xmlrpc_openssl_thread_setup(const char ** const errorP);

void
xmlrpc_openssl_thread_cleanup(void);

#endif
