#ifndef XMLRPC_C_UTIL_INT_H_INCLUDED
#define XMLRPC_C_UTIL_INT_H_INCLUDED

/* This file contains facilities for use by Xmlrpc-c code, but not intended
   to be included in a user compilation.

   Names in here might conflict with other names in a user's compilation
   if included in a user compilation.

   The facilities may change in future releases.
*/

#include "util.h"

/*
  XMLRPC_UTIL_EXPORTED marks a symbol in this file that is exported from
  libxmlrpc_util.

  XMLRPC_BUILDING_UTIL says this compilation is part of libxmlrpc_util, as
  opposed to something that _uses_ libxmlrpc_util.
*/
#ifdef XMLRPC_BUILDING_UTIL
#define XMLRPC_UTIL_EXPORTED XMLRPC_DLLEXPORT
#else
#define XMLRPC_UTIL_EXPORTED
#endif

#ifdef __cplusplus
extern "C" {
#endif

#define MIN(a,b) ((a) < (b) ? (a) : (b))
#define MAX(a,b) ((a) > (b) ? (a) : (b))

/* When we deallocate a pointer in a struct, we often replace it with
** this and throw in a few assertions here and there. */
#define XMLRPC_BAD_POINTER ((void*) 0xDEADBEEF)

/*============================================================================
  xmlrpc_mem_pool

  A memory pool from which you can allocate xmlrpc_mem_block's.

  This is a mechanism for limiting memory allocation.

  Since the xmlrpc_mem_block type is part of the API, we may want to make
  xmlrpc_mem_pool external some day.  For now, any xmlrpc_mem_block created
  outside of Xmlrpc-c code goes in the default pool.
============================================================================*/

typedef struct _xmlrpc_mem_pool xmlrpc_mem_pool;

XMLRPC_UTIL_EXPORTED
xmlrpc_mem_pool *
xmlrpc_mem_pool_new(xmlrpc_env * const envP,
                    size_t       const size);

XMLRPC_UTIL_EXPORTED
void
xmlrpc_mem_pool_free(xmlrpc_mem_pool * const poolP);

XMLRPC_UTIL_EXPORTED
void
xmlrpc_mem_pool_alloc(xmlrpc_env *      const envP, 
                      xmlrpc_mem_pool * const poolP,
                      size_t            const size);

XMLRPC_UTIL_EXPORTED
void
xmlrpc_mem_pool_release(xmlrpc_mem_pool * const poolP,
                        size_t            const size);

XMLRPC_UTIL_EXPORTED
xmlrpc_mem_block *
xmlrpc_mem_block_new_pool(xmlrpc_env *      const envP,
                          size_t            const size,
                          xmlrpc_mem_pool * const poolP);

#ifdef __cplusplus
}
#endif

#endif
