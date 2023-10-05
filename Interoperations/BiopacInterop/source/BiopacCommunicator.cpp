#include "BiopacCommunicator.h"

namespace BiopacInterop
{

	BiopacCommunicator::BiopacCommunicator()
	{
	}



	/*
		Retrieves the type of MP unit to which the server is connected. This may be zero (indicating no unit is connected, e.g. "No Hardware"
		mode) or one of the following: 100, 150, 35, 45. The client can use this value to decide between appropriate templates to download or
		other channel settings.

		Method name: acq.getMPUnitType
		Parameters: None
		Return value: int
	*/
	int BiopacCommunicator::getMPUnitType()
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.getMPUnitType";
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}

	/*
		Retrieves the channels that are available for acquisition and data delivery over a data connection to the client. The type of channels
		that are to be returned are specified in the string parameter. The string parameter may be one of the following: analog, digital, calc.
		The type must be lowercase. "analog" returns information about analog channels, "digital" digital channels, "calc" calculation channels.
		The enabled channels are returned as an array of channel indexes, zero-based. These are the indexes that can be validly used in
		calls for configuring data connection parameters.

		Method name: acq.getEnabledChannels
		Parameters: string
		Return value: array populated with int
	*/
	std::vector<xmlrpc_c::value> BiopacCommunicator::getEnabledChannels(std::string channelType)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.getEnabledChannels";
			xmlrpc_c::paramList params;
			params.add(xmlrpc_c::value_string(channelType));
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			std::vector<xmlrpc_c::value> channels(xmlrpc_c::value_array(result).vectorValueValue());

			return channels;
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return std::vector<xmlrpc_c::value>();
	}


	/*
		Changes the destination machine for data connections, the recipient of the network delivered data. If the parameter is an empty
		string, data connections will automatically be made to the most recently connected client. The empty setting is the default. If
		non-numeric hostnames are specified as parameters, they must be resolvable. If a hostname cannot be resolved, a fault will be
		returned. Proper DNS configuration of the server is beyond the scope of the network data transfer feature.

		Method name: acq.changeDataConnectionHostname
		Parameters: string
		Return value: 0 on success, fault on error
	*/
	int BiopacCommunicator::changeDataConnectionHostname(std::string connectionHostname)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeDataConnectionHostname";
			xmlrpc_c::paramList params;
			params.add(xmlrpc_c::value_string(connectionHostname));
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}


	/*
		Change the transport type that is used to deliver data from the server to the client. The transport type is a string that has one of the
		following values: tcp, udp. XML-RPC last value data delivery may be used in addition to this type provided channels are enabled properly.

		Method name: acq.changeTransportType
		Parameters: string
		Return value: 0 if successful, else fault code
	*/
	int BiopacCommunicator::changeTransportType(std::string transportType)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeTransportType";
			xmlrpc_c::paramList params;
			params.add(xmlrpc_c::value_string(transportType));
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}


	/*
		Changes the method used to deliver data to the client. The parameter is a string that is one of the following: single, multiple.
		"single" opens up a single data connection and sends data in an interleaved fashion. "multiple" opens up an individual data connection
		for each channel.

		Method name: acq.changeDataConnectionMethod
		Parameters: string
		Return value: 0 on success, or fault code
	*/
	int BiopacCommunicator::changeDataConnectionMethod(std::string connectionMethod)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeDataConnectionMethod";
			xmlrpc_c::paramList params;
			params.add(xmlrpc_c::value_string(connectionMethod));
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}

	/*
		Change whether or not data delivery is enabled for a particular channel. Data delivery can only be changed prior to the start of an
		acquisition. Changes to data delivery enabling are only applied on the next start of acquisition.

		Method name: acq.changeDataDeliveryEnabled
		Parameters: channel index parameter structure, boolean
		Return value: 0 for success, else fault code
	*/
	int BiopacCommunicator::changeDataDeliveryEnabled(std::string channelType, int index, bool state)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeDataDeliveryEnabled";
			std::map<std::string, xmlrpc_c::value> channelIndexStruct;
			std::pair<std::string, xmlrpc_c::value> member1("type", xmlrpc_c::value_string(channelType));
			std::pair<std::string, xmlrpc_c::value> member2("index", xmlrpc_c::value_int(index));
			channelIndexStruct.insert(member1);
			channelIndexStruct.insert(member2);

			xmlrpc_c::value_struct param1(channelIndexStruct);
			xmlrpc_c::value_boolean param2(state);

			xmlrpc_c::paramList params;
			params.add(param1);
			params.add(param2);
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}


	/*
		Changes the port on which the individual connection is made by the server to the client to deliver the data for the channel specified
		in the parameters. This style of connection is used only if the data connection method is set to "Multiple".

		Method name: acq.changeDataConnectionPort
		Parameters: channel index parameter structure, integer
		Return value: 0 for success, else fault code
	*/
	int BiopacCommunicator::changeDataConnectionPort(std::string channelType, int index, int port)
	{
		try

		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeDataConnectionPort";
			std::map<std::string, xmlrpc_c::value> channelIndexStruct;
			std::pair<std::string, xmlrpc_c::value> member1("type", xmlrpc_c::value_string(channelType));
			std::pair<std::string, xmlrpc_c::value> member2("index", xmlrpc_c::value_int(index));
			channelIndexStruct.insert(member1);
			channelIndexStruct.insert(member2);

			xmlrpc_c::value_struct param1(channelIndexStruct);
			xmlrpc_c::value_int param2(port);

			xmlrpc_c::paramList params;
			params.add(param1);
			params.add(param2);
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}



	/*
		Changes the data type that is used for binary data streams of the channel's data. The type structure is a struct containing two members.
		The "type" member is one of the following strings: double, float, short. Each string corresponds to the matching C-style data type. The
		"endian" member is one of the following strings: little, big. Each corresponds to the matching byte endian. Bit order within a byte will
		always be big-endian. Not all channels may be able to support all data types. If the channel cannot be transmitted in the requested data
		type, a fault code will be returned.

		Method name: acq.changeDataType
		Parameters: channel index parameter structure, type structure
		Return value: 0 on success, or fault code
	*/
	int BiopacCommunicator::changeDataType(std::string channelType, int index, std::string dataType, std::string dataEndian)
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.changeDataType";

			// build the channel index struct
			std::map<std::string, xmlrpc_c::value> channelIndexStruct;
			std::pair<std::string, xmlrpc_c::value> channelMember1("type", xmlrpc_c::value_string(channelType));
			std::pair<std::string, xmlrpc_c::value> channelMember2("index", xmlrpc_c::value_int(index));
			channelIndexStruct.insert(channelMember1);
			channelIndexStruct.insert(channelMember2);

			// build the data type struct
			std::map<std::string, xmlrpc_c::value> dataTypeStruct;
			std::pair<std::string, xmlrpc_c::value> dataMember1("type", xmlrpc_c::value_string(dataType));
			std::pair<std::string, xmlrpc_c::value> dataMember2("endian", xmlrpc_c::value_string(dataEndian));
			dataTypeStruct.insert(dataMember1);
			dataTypeStruct.insert(dataMember2);

			xmlrpc_c::value_struct param1(channelIndexStruct);
			xmlrpc_c::value_struct param2(dataTypeStruct);

			xmlrpc_c::paramList params;
			params.add(param1);
			params.add(param2);
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, params, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}




	void BiopacCommunicator::setupCommunication()
	{
		std::cout << std::endl << std::endl << std::endl;

		/* XML-RPC SERVER */
		// try to get the MP unit type
		int mpuType = getMPUnitType();
		if (getMPUnitType() > 0)
		{
			std::cout << "XML-RPC SERVER: getMPUnitType() SUCCEEDED" << std::endl << "....." << "unit_type = " << mpuType << std::endl << std::endl;
		}

		// try to load template file
		//if(loadTemplate(xmlrpcTmplPath) == 0)
		//{
		//	std::cout << "XML-RPC SERVER: loadTemplate() SUCCEEDED" << std::endl << "....." << "template_path = " << xmlrpcTmplPath << std::endl << std::endl;
		//}

		// try to get the enabled analog channels
		analogChannels = getEnabledChannels("analog");
		if (analogChannels.size() > 0)
		{
			std::cout << "XML-RPC SERVER: getEnabledChannels() SUCCEEDED" << std::endl << "....." << "analog_channels_indices = [";
			for (unsigned int ch = 0; ch < analogChannels.size(); ch++)
			{
				if (ch == 0)
				{
					std::cout << xmlrpc_c::value_int(analogChannels.at(ch));
				}
				else
				{
					std::cout << "," << xmlrpc_c::value_int(analogChannels.at(ch));
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to get the enabled digital channels
		digitalChannels = getEnabledChannels("digital");
		if (digitalChannels.size() > 0)
		{
			std::cout << "XML-RPC SERVER: getEnabledChannels() SUCCEEDED" << std::endl << "....." << "digital_channels_indices = [";
			for (unsigned int ch = 0; ch < digitalChannels.size(); ch++)
			{
				if (ch == 0)
				{
					std::cout << xmlrpc_c::value_int(digitalChannels.at(ch));
				}
				else
				{
					std::cout << "," << xmlrpc_c::value_int(digitalChannels.at(ch));
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to get the enabled calculation channels
		calcChannels = getEnabledChannels("calc");
		if (calcChannels.size() > 0)
		{
			std::cout << "XML-RPC SERVER: getEnabledChannels() SUCCEEDED" << std::endl << "....." << "calc_channels_indices = [";
			for (unsigned int ch = 0; ch < calcChannels.size(); ch++)
			{
				if (ch == 0)
				{
					std::cout << xmlrpc_c::value_int(calcChannels.at(ch));
				}
				else
				{
					std::cout << "," << xmlrpc_c::value_int(calcChannels.at(ch));
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change data connetion hostname
		if (changeDataConnectionHostname(tcpServer) == 0)
		{
			std::cout << "XML-RPC SERVER: changeDataConnectionHostname() SUCCEEDED" << std::endl << "....." << "connection_hostname = " << tcpServer << std::endl << std::endl;
		}

		// try to change the transport type
		if (changeTransportType("tcp") == 0)
		{
			std::cout << "XML-RPC SERVER: changeTransportType() SUCCEEDED" << std::endl << "....." << "transport_type = tcp" << std::endl << std::endl;
		}

		// try to change the data connection method
		if (changeDataConnectionMethod("multiple") == 0)
		{
			std::cout << "XML-RPC SERVER: changeDataConnectionMethod() SUCCEEDED" << std::endl << "....." << "connection_method = multiple" << std::endl << std::endl;
		}

		// open/close all analog channels for stream
		if (analogChannels.size() > 0)
		{
			std::vector<int> tmp(analogChannelsNum, 0);
			for (unsigned int ch = 0; ch < analogChannels.size(); ch++)
			{
				tmp[xmlrpc_c::value_int(analogChannels.at(ch))] = 1;
			}

			for (unsigned int i = 0; i < tmp.size(); i++)
			{
				if (tmp.at(i) == 1)
				{
					if (changeDataDeliveryEnabled("analog", xmlrpc_c::value_int(i), true) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "analog_channels_indices = [" << xmlrpc_c::value_int(i) << "->open";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->open";
						}
					}
				}
				else
				{
					if (changeDataDeliveryEnabled("analog", xmlrpc_c::value_int(i), false) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "analog_channels_indices = [" << xmlrpc_c::value_int(i) << "->close";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->close";
						}
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened analog channels port
		if (analogChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < analogChannels.size(); ch++)
			{
				if (changeDataConnectionPort("analog", xmlrpc_c::value_int(analogChannels.at(ch)), std::stoi(analogPorts.at(ch))) == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataConnectionPort() SUCCEEDED" << std::endl << "....." << "analog_channels_ports = [" << xmlrpc_c::value_int(analogChannels.at(ch)) << "->" << std::stoi(analogPorts.at(ch));
					}
					else
					{
						std::cout << "," << xmlrpc_c::value_int(analogChannels.at(ch)) << "->" << std::stoi(analogPorts.at(ch));
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened analog channels data type
		if (analogChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < analogChannels.size(); ch++)
			{
				if (changeDataType("analog", xmlrpc_c::value_int(analogChannels.at(ch)), "short", "big") == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataType() SUCCEEDED" << std::endl << "....." << "analog_channels_types = [short:big";
					}
					else
					{
						std::cout << "," << "short:big";
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// open/close all digital channels for stream
		if (digitalChannels.size() > 0)
		{
			std::vector<int> tmp(digitalChannelsNum, 0);
			for (unsigned int ch = 0; ch < digitalChannels.size(); ch++)
			{
				tmp[xmlrpc_c::value_int(digitalChannels.at(ch))] = 1;
			}

			for (unsigned int i = 0; i < tmp.size(); i++)
			{
				if (tmp.at(i) == 1)
				{
					if (changeDataDeliveryEnabled("digital", xmlrpc_c::value_int(i), true) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "digital_channels_indices = [" << xmlrpc_c::value_int(i) << "->open";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->open";
						}
					}
				}
				else
				{
					if (changeDataDeliveryEnabled("digital", xmlrpc_c::value_int(i), false) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "digital_channels_indices = [" << xmlrpc_c::value_int(i) << "->close";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->close";
						}
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened digital channels port
		if (digitalChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < digitalChannels.size(); ch++)
			{
				if (changeDataConnectionPort("digital", xmlrpc_c::value_int(digitalChannels.at(ch)), std::stoi(digitalPorts.at(ch))) == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataConnectionPort() SUCCEEDED" << std::endl << "....." << "digital_channels_ports = [" << xmlrpc_c::value_int(digitalChannels.at(ch)) << "->" << std::stoi(digitalPorts.at(ch));
					}
					else
					{
						std::cout << "," << xmlrpc_c::value_int(digitalChannels.at(ch)) << "->" << std::stoi(digitalPorts.at(ch));
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened digital channels data type
		if (digitalChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < digitalChannels.size(); ch++)
			{
				if (changeDataType("digital", xmlrpc_c::value_int(digitalChannels.at(ch)), "short", "big") == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataType() SUCCEEDED" << std::endl << "....." << "digital_channels_types = [short:big";
					}
					else
					{
						std::cout << "," << "short:big";
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// open/close all calculation channels for stream
		if (calcChannels.size() > 0)
		{
			std::vector<int> tmp(calcChannelsNum, 0);
			for (unsigned int ch = 0; ch < calcChannels.size(); ch++)
			{
				tmp[xmlrpc_c::value_int(calcChannels.at(ch))] = 1;
			}

			for (unsigned int i = 0; i < tmp.size(); i++)
			{
				if (tmp.at(i) == 1)
				{
					if (changeDataDeliveryEnabled("calc", xmlrpc_c::value_int(i), true) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "calc_channels_indices = [" << xmlrpc_c::value_int(i) << "->open";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->open";
						}
					}
				}
				else
				{
					if (changeDataDeliveryEnabled("calc", xmlrpc_c::value_int(i), false) == 0)
					{
						if (i == 0)
						{
							std::cout << "XML-RPC SERVER: changeDataDeliveryEnabled() SUCCEEDED" << std::endl << "....." << "calc_channels_indices = [" << xmlrpc_c::value_int(i) << "->close";
						}
						else
						{
							std::cout << "," << xmlrpc_c::value_int(i) << "->close";
						}
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened calculation channels port
		if (calcChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < calcChannels.size(); ch++)
			{
				if (changeDataConnectionPort("calc", xmlrpc_c::value_int(calcChannels.at(ch)), std::stoi(calcPorts.at(ch))) == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataConnectionPort() SUCCEEDED" << std::endl << "....." << "calc_channels_ports = [" << xmlrpc_c::value_int(calcChannels.at(ch)) << "->" << std::stoi(calcPorts.at(ch));
					}
					else
					{
						std::cout << "," << xmlrpc_c::value_int(calcChannels.at(ch)) << "->" << std::stoi(calcPorts.at(ch));
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}

		// try to change opened calculation channels data type
		if (calcChannels.size() > 0)
		{
			for (unsigned int ch = 0; ch < calcChannels.size(); ch++)
			{
				if (changeDataType("calc", xmlrpc_c::value_int(calcChannels.at(ch)), "double", "big") == 0)
				{
					if (ch == 0)
					{
						std::cout << "XML-RPC SERVER: changeDataType() SUCCEEDED" << std::endl << "....." << "calc_channels_types = [double:big";
					}
					else
					{
						std::cout << "," << "double:big";
					}
				}
			}

			std::cout << "]" << std::endl << std::endl;
		}
	}


	int BiopacCommunicator::startTcpServer()
	{
		std::cout << "TCP SERVER: starting " << std::endl;

		WSADATA wsaData;
		int iResult;

		// initialize Winsock
		iResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
		if (iResult != 0)
		{
			std::cout << "TCP SERVER: WSAStartup failed with error: " << iResult << std::endl;
			return 1;
		}

		std::vector<SOCKET> analogListenSockets(int(analogChannels.size()), INVALID_SOCKET);
		std::vector<SOCKET> analogClientSockets(int(analogChannels.size()), INVALID_SOCKET);

		std::vector<SOCKET> digitalListenSockets(int(digitalChannels.size()), INVALID_SOCKET);
		std::vector<SOCKET> digitalClientSockets(int(digitalChannels.size()), INVALID_SOCKET);

		std::vector<SOCKET> calcListenSockets(int(calcChannels.size()), INVALID_SOCKET);
		std::vector<SOCKET> calcClientSockets(int(calcChannels.size()), INVALID_SOCKET);

		// open sockets for all streaming analog channels
		if (analogChannels.size() > 0)
		{
			struct addrinfo *result = NULL;
			struct addrinfo hints;

			ZeroMemory(&hints, sizeof(hints));
			hints.ai_family = AF_INET;
			hints.ai_socktype = SOCK_STREAM;
			hints.ai_protocol = IPPROTO_TCP;
			hints.ai_flags = AI_PASSIVE;

			for (unsigned int s = 0; s < analogChannels.size(); s++)
			{
				// resolve the server address and port
				iResult = getaddrinfo(tcpServer.c_str(), analogPorts.at(s).c_str(), &hints, &result);
				if (iResult != 0)
				{
					std::cout << "TCP SERVER: getaddrinfo failed with error: " << iResult << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					WSACleanup();
					return 1;
				}

				// create a SOCKET for connecting to server
				analogListenSockets.at(s) = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
				if (analogListenSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: socket failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					freeaddrinfo(result);
					WSACleanup();
					return 1;
				}

				// setup the TCP listening socket
				iResult = bind(analogListenSockets.at(s), result->ai_addr, (int)result->ai_addrlen);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: bind failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					freeaddrinfo(result);
					closesocket(analogListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				freeaddrinfo(result);

				iResult = listen(analogListenSockets.at(s), SOMAXCONN);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: listen failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					closesocket(analogListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// accept a client socket
				analogClientSockets.at(s) = accept(analogListenSockets.at(s), NULL, NULL);
				if (analogClientSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: accept failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					closesocket(analogListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// no longer need server socket
				closesocket(analogListenSockets.at(s));

				std::cout << "TCP SERVER: accepting on port: " << analogPorts.at(s) << " [analog]" << std::endl;
			}
		}

		// open sockets for all streaming digital channels
		if (digitalChannels.size() > 0)
		{
			struct addrinfo *result = NULL;
			struct addrinfo hints;

			ZeroMemory(&hints, sizeof(hints));
			hints.ai_family = AF_INET;
			hints.ai_socktype = SOCK_STREAM;
			hints.ai_protocol = IPPROTO_TCP;
			hints.ai_flags = AI_PASSIVE;

			for (unsigned int s = 0; s < digitalChannels.size(); s++)
			{
				// resolve the server address and port
				iResult = getaddrinfo(tcpServer.c_str(), digitalPorts.at(s).c_str(), &hints, &result);
				if (iResult != 0)
				{
					std::cout << "TCP SERVER: getaddrinfo failed with error: " << iResult << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					WSACleanup();
					return 1;
				}

				// create a SOCKET for connecting to server
				digitalListenSockets.at(s) = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
				if (digitalListenSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: socket failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					freeaddrinfo(result);
					WSACleanup();
					return 1;
				}

				// setup the TCP listening socket
				iResult = bind(digitalListenSockets.at(s), result->ai_addr, (int)result->ai_addrlen);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: bind failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					freeaddrinfo(result);
					closesocket(digitalListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				freeaddrinfo(result);

				iResult = listen(digitalListenSockets.at(s), SOMAXCONN);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: listen failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					closesocket(digitalListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// accept a client socket
				digitalClientSockets.at(s) = accept(digitalListenSockets.at(s), NULL, NULL);
				if (digitalClientSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: accept failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					closesocket(digitalListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// no longer need server socket
				closesocket(digitalListenSockets.at(s));

				std::cout << "TCP SERVER: accepting on port: " << digitalPorts.at(s) << " [digital]" << std::endl;
			}
		}

		// open sockets for all streaming calculation channels
		if (calcChannels.size() > 0)
		{
			struct addrinfo *result = NULL;
			struct addrinfo hints;

			ZeroMemory(&hints, sizeof(hints));
			hints.ai_family = AF_INET;
			hints.ai_socktype = SOCK_STREAM;
			hints.ai_protocol = IPPROTO_TCP;
			hints.ai_flags = AI_PASSIVE;

			for (unsigned int s = 0; s < calcChannels.size(); s++)
			{
				// resolve the server address and port
				iResult = getaddrinfo(tcpServer.c_str(), calcPorts.at(s).c_str(), &hints, &result);
				if (iResult != 0)
				{
					std::cout << "TCP SERVER: getaddrinfo failed with error: " << iResult << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					WSACleanup();
					return 1;
				}

				// create a SOCKET for connecting to server
				calcListenSockets.at(s) = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
				if (calcListenSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: socket failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					freeaddrinfo(result);
					WSACleanup();
					return 1;
				}

				// setup the TCP listening socket
				iResult = bind(calcListenSockets.at(s), result->ai_addr, (int)result->ai_addrlen);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: bind failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					freeaddrinfo(result);
					closesocket(calcListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				freeaddrinfo(result);

				iResult = listen(calcListenSockets.at(s), SOMAXCONN);
				if (iResult == SOCKET_ERROR)
				{
					std::cout << "TCP SERVER: listen failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					closesocket(calcListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// accept a client socket
				calcClientSockets.at(s) = accept(calcListenSockets.at(s), NULL, NULL);
				if (calcClientSockets.at(s) == INVALID_SOCKET)
				{
					std::cout << "TCP SERVER: accept failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					closesocket(calcListenSockets.at(s));
					WSACleanup();
					return 1;
				}

				// no longer need server socket
				closesocket(calcListenSockets.at(s));

				std::cout << "TCP SERVER: accepting on port: " << calcPorts.at(s) << " [calc]" << std::endl;
			}
		}

		std::cout << std::endl;

		// (30 min) X (60 sec) X (2000 samples) X (17 points) = 61200000 elements
		// analog: (61200000 elements) X (2 bytes) = 122400000 bytes = 122400 kilobytes =  122.4 megabytes
		// digital: (61200000 elements) X (2 bytes) = 122400000 bytes = 122400 kilobytes =  122.4 megabytes
		// calculation: (61200000 elements) X (8 bytes) = 489600000 bytes = 489600 kilobytes =  489.6 megabytes

		unsigned int analogDataSize = 10 * analogChannels.size();
		unsigned int digitalDataSize = 10 * digitalChannels.size();
		unsigned int calcDataSize = 10 * calcChannels.size();
		unsigned int dataTimeSize = 10;

		//unsigned int analogDataSize = 30 * 60 * 2000 * analogChannels.size();
		//unsigned int digitalDataSize = 30 * 60 * 2000 * digitalChannels.size();
		//unsigned int calcDataSize = 30 * 60 * 2000 * calcChannels.size();
		//unsigned int dataTimeSize = 30 * 60 * 2000;
		unsigned int allChannelsNum = analogChannels.size() + digitalChannels.size() + calcChannels.size();

		analogData.reserve(analogDataSize);
		digitalData.reserve(digitalDataSize);
		calcData.reserve(calcDataSize);

		dataTime.reserve(dataTimeSize);

		// receive until the peer shuts down the connection
		while (true)
		{
			/*if (analogData.size() > 0)
			{
				std::cout << "c++: " << analogData[analogData.size() - 1] << std::endl;
			}*/

			unsigned int zeroBytes = 0;

			timepoint2 = std::chrono::high_resolution_clock::now();
			std::chrono::duration<double> duration = timepoint2 - timepoint1;

			// get data from calculation channels
			for (unsigned int s = 0; s < calcChannels.size(); s++)
			{
				double buf;
				double sample;

				iResult = recv(calcClientSockets.at(s), reinterpret_cast<char*>(&buf), sizeof(double), 0);

				if (iResult > 0)
				{
					sample = swapDouble(buf);
					calcData.push_back(sample);
				}
				else if (iResult == 0)
				{
					// needs to be implemented with std::nan()
					calcData.push_back(DBL_MIN);
					zeroBytes++;
				}
				else
				{
					std::cout << "TCP SERVER: recv failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
					closesocket(calcClientSockets.at(s));
					WSACleanup();
					return 1;
				}
			}

			// get data from analog channels
			for (unsigned int s = 0; s < analogChannels.size(); s++)
			{
				short buf;
				short sample;

				iResult = recv(analogClientSockets.at(s), reinterpret_cast<char*>(&buf), sizeof(short), 0);

				if (iResult > 0)
				{
					sample = swapShort(buf);
					analogData.push_back(sample);
				}
				else if (iResult == 0)
				{
					// needs to be implemented with std::nan(), but it is short!!
					analogData.push_back(SHRT_MIN);
					zeroBytes++;
				}
				else
				{
					std::cout << "TCP SERVER: recv failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
					closesocket(analogClientSockets.at(s));
					WSACleanup();
					return 1;
				}
			}

			// get data from digital channels
			for (unsigned int s = 0; s < digitalChannels.size(); s++)
			{
				short buf;
				short sample;

				iResult = recv(digitalClientSockets.at(s), reinterpret_cast<char*>(&buf), sizeof(short), 0);

				if (iResult > 0)
				{
					sample = swapShort(buf);
					digitalData.push_back(sample);
				}
				else if (iResult == 0)
				{
					// needs to be implemented with std::nan(), but it is short!!
					digitalData.push_back(SHRT_MIN);
					zeroBytes++;
				}
				else
				{
					std::cout << "TCP SERVER: recv failed with error: " << WSAGetLastError() << std::endl;
					std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
					closesocket(digitalClientSockets.at(s));
					WSACleanup();
					return 1;
				}
			}

			dataTime.push_back(duration.count());

			if (zeroBytes == allChannelsNum)
			{
				std::cout << "TCP SERVER: closing" << std::endl;

				// cleanup
				for (unsigned int s = 0; s < analogChannels.size(); s++)
				{
					// shutdown the connection
					iResult = shutdown(analogClientSockets.at(s), SD_SEND);
					if (iResult == SOCKET_ERROR)
					{
						std::cout << "TCP SERVER: shutdown failed with error: " << WSAGetLastError() << std::endl;
						std::cout << "TCP SERVER: socket at port: " << analogPorts.at(s) << std::endl;
						closesocket(analogClientSockets.at(s));
						WSACleanup();
						return 1;
					}

					closesocket(analogClientSockets.at(s));

					std::cout << "TCP SERVER: shutting on port: " << analogPorts.at(s) << " [analog]" << std::endl;
				}

				// cleanup
				for (unsigned int s = 0; s < digitalChannels.size(); s++)
				{
					// shutdown the connection
					iResult = shutdown(digitalClientSockets.at(s), SD_SEND);
					if (iResult == SOCKET_ERROR)
					{
						std::cout << "TCP SERVER: shutdown failed with error: " << WSAGetLastError() << std::endl;
						std::cout << "TCP SERVER: socket at port: " << digitalPorts.at(s) << std::endl;
						closesocket(digitalClientSockets.at(s));
						WSACleanup();
						return 1;
					}

					closesocket(digitalClientSockets.at(s));

					std::cout << "TCP SERVER: shutting on port: " << digitalPorts.at(s) << " [digital]" << std::endl;
				}

				// cleanup
				for (unsigned int s = 0; s < calcChannels.size(); s++)
				{
					// shutdown the connection
					iResult = shutdown(calcClientSockets.at(s), SD_SEND);
					if (iResult == SOCKET_ERROR)
					{
						std::cout << "TCP SERVER: shutdown failed with error: " << WSAGetLastError() << std::endl;
						std::cout << "TCP SERVER: socket at port: " << calcPorts.at(s) << std::endl;
						closesocket(calcClientSockets.at(s));
						WSACleanup();
						return 1;
					}

					closesocket(calcClientSockets.at(s));

					std::cout << "TCP SERVER: shutting on port: " << calcPorts.at(s) << " [calc]" << std::endl;
				}

				std::cout << std::endl;

				WSACleanup();
				return 0;
			}
		}

		return 1;
	}

	/*
	Toggles data acquisition in the frontmost graph. If data acquisition is in progress, it is halted. If none is in progress, data acquisition
	is started in the graph. Note that this function invocation may block if physical user interaction is required to start the acquisition in
	the graph, such as dismissing an overwrite warning, warnings on incompatibilities between different MP unit types, specifying a save location
	for acquisition to disk, etc. If the implementation of the XML-RPC binding used by the client supports timeout capabilities, it is highly
	recommended to enable timeouts for this function.

	Method name: acq.toggleAcquisition
	Parameters: none
	Return value: 0 on success, else fault code
*/
	int BiopacCommunicator::toggleAcquisition()
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.toggleAcquisition";
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, &result);

			return xmlrpc_c::value_int(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}


	/*
	Query whether data acquisition is currently in progress or not. A value of true is returned if data acquisition is occurring in any
	open AcqKnowledge graph window.

	Method name: acq.getAcquisitionInProgress
	Parameters: none
	Return value: boolean
*/
	int BiopacCommunicator::getAcquisitionInProgress()
	{
		try
		{
			xmlrpc_c::value result;
			xmlrpc_c::clientSimple xmlrpcClient;
			std::string xmlrpcMethod = "acq.getAcquisitionInProgress";
			xmlrpcClient.call(xmlrpcServer, xmlrpcMethod, &result);

			return (int)xmlrpc_c::value_boolean(result);
		}
		catch (std::exception const& e)
		{
			std::cerr << "Client threw error: " << e.what() << std::endl;
		}
		catch (...)
		{
			std::cerr << "Client threw unexpected error" << std::endl;
		}

		return -1;
	}



	int BiopacCommunicator::getData()
	{
		return analogData.front();
	}

	void BiopacCommunicator::startCommunication()
	{
		setupCommunication();
		startTcpServer();
	}

	BiopacCommunicator::~BiopacCommunicator()
	{
	}

}
