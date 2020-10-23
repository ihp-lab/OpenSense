#ifndef LOCK_H_INCLUDED
#define LOCK_H_INCLUDED

struct lock;

/* GCC 2.95.3 <iostream> defines a function called 'lock', so we cannot
   typedef struct lock to 'lock' here.
*/

typedef void lockAcquireFn(struct lock *);
typedef void lockReleaseFn(struct lock *);
typedef void lockDestroyFn(struct lock *);

struct lock {
    /* To finish the job of making an abstract lock class that can use locks
       other than pthread mutexes, we need to replace 'theLock' with a
       "void * implementationP" and make curlLock_create_pthread() malloc
       the mutex.
    */
    void * implementationP;
    lockAcquireFn * acquire;
    lockReleaseFn * release;
    lockDestroyFn * destroy;
};

#endif
