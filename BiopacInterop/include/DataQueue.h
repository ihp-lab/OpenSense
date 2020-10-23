#pragma once
#include <deque>

namespace BiopacInterop
{
	template<class T>
	class DataQueue
	{
	public:
		DataQueue();
		~DataQueue();
		T front();
		T back();
		void push_back(T val);
		void reserve(int size) { capacity = size; }
	private:
		int capacity;
		std::deque<T> data;
	};




	template<typename T>
	DataQueue<T>::DataQueue()
	{
		capacity = 100;
	}

	template<typename T>
	T DataQueue<T>::front()
	{
		T firstVal = -1;
		if (!data.empty())
		{
			firstVal = data.front();
		}
		return firstVal;
	}

	template<typename T>
	T DataQueue<T>::back()
	{
		
		T lastVal = -1;
		if (!data.empty())
		{
			lastVal = data.back();
		}
		return lastVal;
	}
	struct test
	{
		int val = 0;
	};
	template<typename T>
	void DataQueue<T>::push_back(T val)
	{
		if (data.size() >= capacity)
		{
			data.pop_front();
		}
		data.push_back(val);
	}

	template<typename T>
	DataQueue<T>::~DataQueue()
	{

	}

}