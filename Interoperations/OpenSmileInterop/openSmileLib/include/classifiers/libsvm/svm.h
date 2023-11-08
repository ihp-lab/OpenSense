/*
LibSVM header file

Copyright (c) 2000-2008 Chih-Chung Chang and Chih-Jen Lin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

3. Neither name of copyright holders nor the names of its contributors
may be used to endorse or promote products derived from this software
without specific prior written permission.


THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


***************************
Changes from original code:
***************************

2009:

* This header file was modified by Florian Eyben in order to better
  integrate LibSVM into openSMILE

* Functions for saving and loading binary SVM model files were added
  by Florian Eyben

*/


#ifndef _LIBSVM_H
#define _LIBSVM_H

#define LIBSVM_VERSION 287

#ifndef NOSMILE
#include <core/smileCommon.hpp>
#else
#define DLLEXPORT
#define BUILD_LIBSVM
#endif

#ifdef BUILD_LIBSVM

#ifdef __cplusplus
extern "C" {
#endif

struct svm_node
{
	int index;
	double value;
};

struct svm_problem
{
	int l;
	double *y;
	struct svm_node **x;
};

enum { C_SVC, NU_SVC, ONE_CLASS, EPSILON_SVR, NU_SVR };	/* svm_type */
enum { LINEAR, POLY, RBF, SIGMOID, PRECOMPUTED }; /* kernel_type */

struct svm_parameter
{
	int svm_type;
	int kernel_type;
	int degree;	/* for poly */
	double gamma;	/* for poly/rbf/sigmoid */
	double coef0;	/* for poly/sigmoid */

	/* these are for training only */
	double cache_size; /* in MB */
	double eps;	/* stopping criteria */
	double C;	/* for C_SVC, EPSILON_SVR and NU_SVR */
	int nr_weight;		/* for C_SVC */
	int *weight_label;	/* for C_SVC */
	double* weight;		/* for C_SVC */
	double nu;	/* for NU_SVC, ONE_CLASS, and NU_SVR */
	double p;	/* for EPSILON_SVR */
	int shrinking;	/* use the shrinking heuristics */
	int probability; /* do probability estimates */
};

DLLEXPORT struct svm_model *svm_train(const struct svm_problem *prob, const struct svm_parameter *param);
DLLEXPORT void svm_cross_validation(const struct svm_problem *prob, const struct svm_parameter *param, int nr_fold, double *target);

DLLEXPORT int svm_save_binary_model(const char *model_file_name, const svm_model *model);
DLLEXPORT struct svm_model *svm_load_binary_model(const char *model_file_name);

DLLEXPORT struct svm_model *svm_load_ascii_model(const char *model_file_name);
/* autodetect binary/ascii model: */
DLLEXPORT struct svm_model *svm_load_model(const char *model_file_name);

DLLEXPORT int svm_save_model(const char *model_file_name, const struct svm_model *model);

DLLEXPORT int svm_get_svm_type(const struct svm_model *model);
DLLEXPORT int svm_get_sv_size(const struct svm_model *model);
DLLEXPORT int svm_get_nr_class(const struct svm_model *model);
DLLEXPORT void svm_get_labels(const struct svm_model *model, int *label);
DLLEXPORT double svm_get_svr_probability(const struct svm_model *model);

DLLEXPORT void svm_predict_values(const struct svm_model *model, const struct svm_node *x, double* dec_values);
DLLEXPORT double svm_predict(const struct svm_model *model, const struct svm_node *x);
DLLEXPORT double svm_predict_probability(const struct svm_model *model, const struct svm_node *x, double* prob_estimates);

DLLEXPORT void svm_destroy_model(struct svm_model *model);
DLLEXPORT void svm_destroy_param(struct svm_parameter *param);

DLLEXPORT const char *svm_check_parameter(const struct svm_problem *prob, const struct svm_parameter *param);
DLLEXPORT int svm_check_probability_model(const struct svm_model *model);

#ifdef __cplusplus
}
#endif

#endif // #ifdef BUILD_LIBSVM

#endif /* _LIBSVM_H */
