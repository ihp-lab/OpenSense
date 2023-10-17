/*F***************************************************************************
 * 
 * openSMILE - the Munich open source Multimedia Interpretation by 
 * Large-scale Extraction toolkit
 * 
 * This file is part of openSMILE.
 * 
 * openSMILE is copyright (c) by audEERING GmbH. All rights reserved.
 * 
 * See file "COPYING" for details on usage rights and licensing terms.
 * By using, copying, editing, compiling, modifying, reading, etc. this
 * file, you agree to the licensing terms in the file COPYING.
 * If you do not agree to the licensing terms,
 * you must immediately destroy all copies of this file.
 * 
 * THIS SOFTWARE COMES "AS IS", WITH NO WARRANTIES. THIS MEANS NO EXPRESS,
 * IMPLIED OR STATUTORY WARRANTY, INCLUDING WITHOUT LIMITATION, WARRANTIES OF
 * MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE, ANY WARRANTY AGAINST
 * INTERFERENCE WITH YOUR ENJOYMENT OF THE SOFTWARE OR ANY WARRANTY OF TITLE
 * OR NON-INFRINGEMENT. THERE IS NO WARRANTY THAT THIS SOFTWARE WILL FULFILL
 * ANY OF YOUR PARTICULAR PURPOSES OR NEEDS. ALSO, YOU MUST PASS THIS
 * DISCLAIMER ON WHENEVER YOU DISTRIBUTE THE SOFTWARE OR DERIVATIVE WORKS.
 * NEITHER TUM NOR ANY CONTRIBUTOR TO THE SOFTWARE WILL BE LIABLE FOR ANY
 * DAMAGES RELATED TO THE SOFTWARE OR THIS LICENSE AGREEMENT, INCLUDING
 * DIRECT, INDIRECT, SPECIAL, CONSEQUENTIAL OR INCIDENTAL DAMAGES, TO THE
 * MAXIMUM EXTENT THE LAW PERMITS, NO MATTER WHAT LEGAL THEORY IT IS BASED ON.
 * ALSO, YOU MUST PASS THIS LIMITATION OF LIABILITY ON WHENEVER YOU DISTRIBUTE
 * THE SOFTWARE OR DERIVATIVE WORKS.
 * 
 * Main authors: Florian Eyben, Felix Weninger, 
 *            Martin Woellmer, Bjoern Schuller
 * 
 * Copyright (c) 2008-2013, 
 *   Institute for Human-Machine Communication,
 *   Technische Universitaet Muenchen, Germany
 * 
 * Copyright (c) 2013-2015, 
 *   audEERING UG (haftungsbeschraenkt),
 *   Gilching, Germany
 * 
 * Copyright (c) 2016,   
 *   audEERING GmbH,
 *   Gilching Germany
 ***************************************************************************E*/

#ifndef SMILEUTIL_JSONCLASSES_HPP
#define SMILEUTIL_JSONCLASSES_HPP

#include <smileutil/JsonClassesForward.hpp>
#include <rapidjson/document.h>
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>
#include <rapidjson/reader.h>

namespace smileutil {
namespace json {
    class JsonValue
    {
    private:
        mutable rapidjson::Value *_p;

    public:
        JsonValue()
            : _p(NULL)
        {
        }

        JsonValue(rapidjson::Value *p)
            : _p(p)
        {
        }

        bool isValid() const
        {
            return (_p != NULL);
        }

        rapidjson::Value* operator-> () const
        {
            return _p;
        }

        rapidjson::Value& operator* () const
        {
            return *_p;
        }
    };

    class JsonAllocator
    {
    private:
        mutable rapidjson::MemoryPoolAllocator<> *_p;

    public:
        JsonAllocator(rapidjson::MemoryPoolAllocator<> *p)
            : _p(p)
        {
        }

        rapidjson::MemoryPoolAllocator<>* operator-> () const
        {
            return _p;
        }

        operator rapidjson::MemoryPoolAllocator<>& () const
        {
            return *_p;
        }
    };

    class JsonDocument
    {
    private:
        mutable rapidjson::Document *_p;

    public:
        JsonDocument(rapidjson::Document *p)
            : _p(p)
        {
        }

        JsonDocument(rapidjson::Document &p)
            : _p(&p)
        {
        }

        rapidjson::Document* operator-> () const
        {
            return _p;
        }

        rapidjson::Document& operator* () const
        {
            return *_p;
        }

        operator rapidjson::Document& () const
        {
            return *_p;
        }
    };

}  // namespace json
}  // namespace smileutil


#endif // SMILEUTIL_JSONCLASSES_HPP
