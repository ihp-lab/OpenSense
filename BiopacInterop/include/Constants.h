#pragma once

#include <vector>

static const int analogChannelsNum = 16;
static const int digitalChannelsNum = 16;
static const int calcChannelsNum = 16;

// DEFAULT ACQKNOWLEDGE PORTS: analog channels [15020-15035]; digital channels [15040-15055]; calculation channels [15060-15075]
static const std::string tcpServer = "127.0.0.1";
static const char* analogPortsArr[] = { "15020", "15021", "15022", "15023", "15024", "15025", "15026", "15027", "15028", "15029", "15030", "15031", "15032", "15033", "15034", "15035" };
static const char* digitalPortsArr[] = { "15040", "15041", "15042", "15043", "15044", "15045", "15046", "15047", "15048", "15049", "15050", "15051", "15052", "15053", "15054", "15055" };
static const char* calcPortsArr[] = { "15060", "15061", "15062", "15063", "15064", "15065", "15066", "15067", "15068", "15069", "15070", "15071", "15072", "15073", "15074", "15075" };
static const std::vector<std::string> analogPorts(analogPortsArr, analogPortsArr + sizeof(analogPortsArr) / sizeof(analogPortsArr[0]));
static const std::vector<std::string> digitalPorts(digitalPortsArr, digitalPortsArr + sizeof(digitalPortsArr) / sizeof(digitalPortsArr[0]));
static const std::vector<std::string> calcPorts(calcPortsArr, calcPortsArr + sizeof(calcPortsArr) / sizeof(calcPortsArr[0]));

static const std::string xmlrpcServer = "http://localhost:15010/RPC2";