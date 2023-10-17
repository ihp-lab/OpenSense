#pragma once

/*
	Swaps the bit order in a double.

	Method name: swapDouble
	Parameters: double
	Return value: double
*/
double swapDouble(double d)
{
	double a;

	unsigned char *dst = (unsigned char *)&a;
	unsigned char *src = (unsigned char *)&d;

	dst[0] = src[7];
	dst[1] = src[6];
	dst[2] = src[5];
	dst[3] = src[4];
	dst[4] = src[3];
	dst[5] = src[2];
	dst[6] = src[1];
	dst[7] = src[0];

	return a;
}

/*
	Swaps the bit order in a short.

	Method name: swapShort
	Parameters: short
	Return value: short
*/
short swapShort(short s)
{
	short a;

	unsigned char *dst = (unsigned char *)&a;
	unsigned char *src = (unsigned char *)&s;

	dst[0] = src[1];
	dst[1] = src[0];

	return a;
}
