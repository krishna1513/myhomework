#include "questions.h"

int UsefulFunctions::OddNumberFromArray()
{
	int fs = 4;
	int ss = 5;
	int first[4] = {2,4,6,8};
	int second[5] = {2,4,6,10,8};
	
	int sumf = 0;
	int sums = 0;

	for(int i = 0;i< 4;i++)
	{
		sumf += first[i];
		sums += second[i];
	}

	for(int i = ss -1;i>fs-1;i--)
	{
		sums += second[i];
	}

	return sums-sumf;
}
