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
 * 	      Martin Woellmer, Bjoern Schuller
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


/*  openSMILE component:

TUM keyword spotter (Julius version)

*/


#ifndef __CJULIUSSINK_HPP
#define __CJULIUSSINK_HPP

#include <core/smileCommon.hpp>

//#define HAVE_JULIUSLIB
#ifdef HAVE_JULIUSLIB

#include <core/dataSink.hpp>

// STL includes for the queue
#include <queue>

// apr seems to define its own true, so we undefine it here... (we just have to make sure that we include this header file after any other apr/semaine/etc. headers
#ifdef TRUE
#undef TRUE
#endif 

extern "C" {
#undef LOG_DEBUG
#include <julius/juliuslib.h>
// julius defines min, max, abs, which collides with GNU stdlib and XQilla abs()
// there might be more of these silly lowercase(!) defines... Isn't it common practice to have all defines in uppercase where possible??
#undef abs
#undef min
#undef max
}


#define COMPONENT_DESCRIPTION_CJULIUSSINK "keyword spotter using Julius decoder"
#define COMPONENT_NAME_CJULIUSSINK "cJuliusSink"

#define MAX_PHONEME_STRLEN  5

#include <classifiers/julius/kwsjKresult.h>

#undef class




class DLLEXPORT cJuliusSink : public cDataSink {
  private:
    TurnMsgQueue tsq;   // turn start queue
    TurnMsgQueue teq;   // turn end queue
    int maxTurnQue;
    int running;
    int nopass2;
    int offlineMode;
    int noTurns;
    int purgeQue;
    long midx;

    const char * configfile;
    smileMutex terminatedMtx;
    smileThread decoderThread;
    smileMutex dataFlgMtx;
    smileCond  tickCond;
    int juliusIsSetup, juliusIsRunning;
    double period;

    int sendKwsResult;
    int sendJuliusResult;
    const char * kwsResultRecp;
    const char * juliusResultRecp;

 

    // number of silence frames to keep at beginning and end of turn
    int preSil, postSil;
    long lag, endWait;
    int nPre, nPost;
    long curVidx, vIdxStart, vIdxEnd;

	  const char * logfile;
    FILE *fp;
    Recog *recog;
    Jconf *jconf;

    int decoderThreadPriority;

    bool terminated;
    int dataFlag;
    int turnEnd; int turnStart; int isTurn;
    const cVector *curVec;

    double turnStartSmileTime, turnStartSmileTimeLast, turnStartSmileTimeCur;

    const char *keywordList;
    char **keywords;
    int kwIndexStart[27*26];
    int kwIndexEnd[27*26];
    int numKw;
    int nExclude;
    const char **excludeWords;

    /*
    char * sentence;
    int speakingIndex,seqIdx;
    int nFeaturesSelected;
    int *featureIndex;
    long long curTime;
    int wasSpeaking;
  */  
    
/*
int prevSpeakingIndex;
    int thisSpeakingIndex;
    int countdown;
    bool outputtrigger;
*/

    /* required by julius output functions ? */
    int writelen;
    int wst;

    int setupJulius();
    int startJuliusDecoder();

    void setupCallbacks(Recog *recog, void *data);

//    void juPutHypoPhoneme(WORD_ID *seq, int n, WORD_INFO *winfo);
    void juAlignPass1Keywords(RecogProcess *r, HTK_Param *param);

    int excludeWord(const char * w);
    juliusResult * fillJresult(Sentence *s, WORD_INFO *winfo, int numOfFrames);
    void loadKeywordList();
    int makeKwIndex(const char *kw);
    int isKeyword(const char *word);
    int tagKeywords(juliusResult *r);
    void keywordFilter(juliusResult *r);

    int checkMessageQueue();
    
    
  protected:
    SMILECOMPONENT_STATIC_DECL_PR
    int printResult;

    virtual void fetchConfig();
    //virtual int myConfigureInstance();
    virtual int myFinaliseInstance();
    virtual int myTick(long long t);

    //virtual void processResult(long long tick, long frameIdx, double time);
    virtual int processComponentMessage( cComponentMessage *_msg );

  public:
    //static sComponentInfo * registerComponent(cConfigManager *_confman);
    //static cSmileComponent * create(const char *_instname);
    SMILECOMPONENT_STATIC_DECL

   
    void ATKresultThread();
    cJuliusSink(const char *_name);

    void juliusDecoderThread();

      
      /* callbacks for julius : */

    int getFv(float *vec, int n);

    LOGPROB cbUserlmUni(WORD_INFO *winfo, WORD_ID w, LOGPROB ngram_prob);
    LOGPROB cbUserlmBi(WORD_INFO *winfo, WORD_ID context, WORD_ID w, LOGPROB ngram_prob);
    LOGPROB cbUserlmLm(WORD_INFO *winfo, WORD_ID *contexts, int clen, WORD_ID w, LOGPROB ngram_prob);

    void cbResultPass1(Recog *recog, void *dummy);
    void cbResultPass1Current(Recog *recog, void *dummy);
    void cbStatusPass1Begin(Recog *recog, void *dummy);

    void cbResultPass2(Recog *recog, void *dummy);

    virtual ~cJuliusSink();
};



#endif //HAVE_JULIUSLIB



#endif // __CJULIUSSINK_HPP
