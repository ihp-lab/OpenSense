#pragma once

#define WIN32_LEAN_AND_MEAN

#include "Constants.h"
#include "ManagedObject.h"
#include <iostream>
//#include <thread>
#include <winsock2.h>
#include <ws2tcpip.h>
#include "Utilities.h"
#include <vector>
#include <deque>
#include <chrono>
#include "DataQueue.h"

#include <xmlrpc-c/base.hpp>
#include <xmlrpc-c/client_simple.hpp>
using namespace System;
using namespace System::Threading;

namespace BiopacInterop
{
	class BiopacCommunicator
	{
	private:
		int getMPUnitType();
		int changeDataConnectionHostname(std::string connectionHostname);
		int changeTransportType(std::string transportType);
		int changeDataConnectionMethod(std::string connectionMethod);
		std::vector<xmlrpc_c::value> getEnabledChannels(std::string channelType);
		int changeDataDeliveryEnabled(std::string channelType, int index, bool state);
		int changeDataConnectionPort(std::string channelType, int index, int port);
		int changeDataType(std::string channelType, int index, std::string dataType, std::string dataEndian);

		std::vector<xmlrpc_c::value> analogChannels;
		std::vector<xmlrpc_c::value> digitalChannels;
		std::vector<xmlrpc_c::value> calcChannels;

		DataQueue<short> analogData;
		DataQueue<short> digitalData;
		DataQueue<double> calcData;
		DataQueue<double> dataTime;

		std::chrono::high_resolution_clock::time_point timepoint1, timepoint2, timepoint3;


	public:

		BiopacCommunicator();
		void setupCommunication();
		int toggleAcquisition();
		int startTcpServer();
		int getAcquisitionInProgress();

		int getData();
		void startCommunication();

		~BiopacCommunicator();

	};

	public ref class BiopacCommunicatorWrapper : ManagedObject<BiopacInterop::BiopacCommunicator>
	{
	public:
		BiopacCommunicatorWrapper()
			: ManagedObject(new BiopacInterop::BiopacCommunicator())
		{

		}

		void StartCommunicationDelegateFunction()
		{
			m_Instance->startCommunication();
		}

		void StartCommunication()
		{
			ThreadStart^ startDelegate = gcnew ThreadStart(this, &BiopacCommunicatorWrapper::StartCommunicationDelegateFunction);
			Thread^ thread = gcnew Thread(startDelegate);
			thread->Start();
			System::Threading::Thread::Sleep(TimeSpan::FromSeconds(5));

			if (m_Instance->getAcquisitionInProgress() != 1)
			{
				if (m_Instance->toggleAcquisition() == 0)
				{
					Console::WriteLine("XML-RPC SERVER: toggleAcquisition() SUCCEEDED" + "\n" + "....." + "acquisition_progress = on");
				}
			}
			else
			{
				m_Instance->toggleAcquisition();
				if (m_Instance->toggleAcquisition() == 0)
				{
					Console::WriteLine("XML-RPC SERVER: toggleAcquisition() SUCCEEDED" + "\n" + "....." + "acquisition_progress = on");
				}
			}
		}
		int GetData()
		{
			return m_Instance->getData();
		}

		int toggleAcquisition()
		{
			return m_Instance->toggleAcquisition();
		}

		int getAcquisitionInProgress()
		{
			return m_Instance->getAcquisitionInProgress();
		}
	};
}

