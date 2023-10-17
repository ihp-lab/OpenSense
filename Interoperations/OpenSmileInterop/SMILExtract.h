#pragma once

#include <core/smileCommon.hpp>
#include <core/configManager.hpp>
#include <core/commandlineParser.hpp>

#include "ComponentManager.h"

#define MODULE "SMILExtract"

using namespace System;

namespace OpenSmileInterop {

	public ref class SMILExtract {
	private:
		property array<String^>^ args;

	public:
		SMILExtract(array<String^>^ args) {
			this->args = args;
		}

		SMILExtract() : SMILExtract(gcnew array<String^>(0)) {}

		long long Execute() {
			return Execute(this->args);
		}

		long long static Execute(array<String^>^ args) {
			// construct argc & argv
			auto argc = args->Length;
			char ** tokensAsUtf8 = new char*[args->Length]; // openSmileLib is built using utf-8 rather than Unicode.
			for (auto i = 0; i < args->Length; i++) {
				auto encodedBytes = Text::Encoding::UTF8->GetBytes(args[i]);

				// Probably just using [0] is fine here
				pin_ptr<Byte> pinnedBytes = &encodedBytes[encodedBytes->GetLowerBound(0)];

				tokensAsUtf8[i] = new char[encodedBytes->Length + 1];
				memcpy(tokensAsUtf8[i], reinterpret_cast<char*>(pinnedBytes), encodedBytes->Length);

				// NULL-terminate the native string
				tokensAsUtf8[i][encodedBytes->Length] = '\0';

			}
			const char ** argv = (const char ** )tokensAsUtf8;
			// SMILExtract main
			try {

				smileCommon_fixLocaleEnUs();

				// set up the smile logger
				LOGGER.setLogLevel(1);
				LOGGER.enableConsoleOutput();

				// commandline parser:
				cCommandlineParser cmdline(argc, argv);
				cmdline.addStr("configfile", 'C', "Path to openSMILE config file", "smile.conf");
				cmdline.addInt("loglevel", 'l', "Verbosity level (0-9)", 2);
#ifdef DEBUG
				cmdline.addBoolean("debug", 'd', "Show debug messages (on/off)", 0);
#endif
				cmdline.addInt("nticks", 't', "Number of ticks to process (-1 = infinite) (only works for single thread processing, i.e. nThreads=1)", -1);
				//cmdline.addBoolean( "configHelp", 'H', "Show documentation of registered config types (on/off)", 0 );
				cmdline.addBoolean("components", 'L', "Show component list", 0);
				cmdline.addStr("configHelp", 'H', "Show documentation of registered config types (on/off/argument) (if an argument is given, show only documentation for config types beginning with the name given in the argument)", NULL, 0);
				cmdline.addStr("configDflt", 0, "Show default config section templates for each config type (on/off/argument) (if an argument is given, show only documentation for config types beginning with the name given in the argument, OR for a list of components in conjunctions with the 'cfgFileTemplate' option enabled)", NULL, 0);
				cmdline.addBoolean("cfgFileTemplate", 0, "Print a complete template config file for a configuration containing the components specified in a comma separated string as argument to the 'configDflt' option", 0);
				cmdline.addBoolean("cfgFileDescriptions", 0, "Include description in config file templates.", 0);
				cmdline.addBoolean("ccmdHelp", 'c', "Show custom commandline option help (those specified in config file)", 0);
				cmdline.addStr("logfile", 0, "set log file", "smile.log");
				cmdline.addBoolean("nologfile", 0, "don't write to a log file (e.g. on a read-only filesystem)", 0);
				cmdline.addBoolean("noconsoleoutput", 0, "don't output any messages to the console (log file is not affected by this option)", 0);
				cmdline.addBoolean("appendLogfile", 0, "append log messages to an existing logfile instead of overwriting the logfile at every start", 0);

				bool help = false;
				if (cmdline.doParse() == -1) {
					LOGGER.setLogLevel(0);
					help = true;
				}
				if (argc <= 1) {
					throw gcnew System::Exception("No commandline options were given.\n Please run ' SMILExtract -h ' to see some usage information!\n\n");
				}

				if (help == true) {

					throw gcnew System::Exception("Help_1");
				}

				if (cmdline.getBoolean("nologfile")) {
					LOGGER.setLogFile((const char *)NULL, 0, !(cmdline.getBoolean("noconsoleoutput")));
				} else {
					LOGGER.setLogFile(cmdline.getStr("logfile"), cmdline.getBoolean("appendLogfile"), !(cmdline.getBoolean("noconsoleoutput")));
				}
				LOGGER.setLogLevel(cmdline.getInt("loglevel"));
				SMILE_MSG(2, "openSMILE starting!");

#ifdef DEBUG  // ??
				if (!cmdline.getBoolean("debug"))
					LOGGER.setLogLevel(LOG_DEBUG, 0);
#endif

				SMILE_MSG(2, "config file is: %s", cmdline.getStr("configfile"));


				// create configManager:
				cConfigManager *configManager = new cConfigManager(&cmdline);


				cComponentManager *cMan = new cInteropComponentManager(configManager);

				try {
					const char *selStr = NULL;
					if (cmdline.isSet("configHelp")) {
#ifndef EXTERNAL_BUILD
						selStr = cmdline.getStr("configHelp");
						configManager->printTypeHelp(1/*!!! -> 1*/, selStr, 0);
#endif
						help = true;
					}
					if (cmdline.isSet("configDflt")) {
#ifndef EXTERNAL_BUILD
						int fullMode = 0;
						int wDescr = 0;
						if (cmdline.getBoolean("cfgFileTemplate")) fullMode = 1;
						if (cmdline.getBoolean("cfgFileDescriptions")) wDescr = 1;
						selStr = cmdline.getStr("configDflt");
						configManager->printTypeDfltConfig(selStr, 1, fullMode, wDescr);
#endif
						help = true;
					}
					if (cmdline.getBoolean("components")) {
#ifndef EXTERNAL_BUILD
						cMan->printComponentList();
#endif  // EXTERNAL_BUILD
						help = true;
					}

					if (help == true) {
						throw gcnew System::Exception("Help_2");
					}

					configManager->addReader(new cFileConfigReader(cmdline.getStr("configfile"), -1, &cmdline));
					configManager->readConfig();

					/* re-parse the command-line to include options created in the config file */
					cmdline.doParse(1, 0); // warn if unknown options are detected on the commandline
					if (cmdline.getBoolean("ccmdHelp")) {
						cmdline.showUsage();
						throw gcnew System::Exception("Help_3");
					}

					/* create all instances specified in the config file */
					cMan->createInstances(0); // 0 = do not read config (we already did that above..)

					/* run single or mutli-threaded, depending on componentManager config in config file */
					long long nTicks = cMan->runMultiThreaded(cmdline.getInt("nticks"));

					return nTicks;
				} finally{
					delete configManager;
					delete cMan;
				}
			} catch (cSMILException *c) {
				throw gcnew System::Exception(gcnew String(c->getText()));
			} finally{
				for (auto i = 0; i < args->Length; i++) {
					delete[] tokensAsUtf8[i];
				}
				delete[] tokensAsUtf8;
			}
		}

	};
}

#undef MODULE
