#include "FFT.h"

#include <memory.h>
#include <malloc.h>

#define _USE_MATH_DEFINES
#include <math.h>

#include "DFT.h"

int Log2I(int num)
{
	int result = 30;
	int n = 0x01 << 30;
	while (n > 0)
	{
		if ((num & n) != 0)
			return result;

		n >>= 1;
		result--;
	}

	return -1;
}

void PermutationComplex(Complex *array, int count)
{
	int length = count;

	int nInv = 0;
	int initialBitMask = 0x01 << Log2I(length - 1);
	for (int n = 0; n < length; n++)
	{
		if (n < nInv)
		{
			Complex temp = array[n];
			array[n] = array[nInv];
			array[nInv] = temp;
		}

		int bitMask = initialBitMask;
		while (bitMask > 0)
		{
			if ((nInv & bitMask) != 0)
			{
				nInv = nInv & ~bitMask;
				bitMask >>= 1;
			}
			else
			{
				nInv |= bitMask;
				break;
			}
		}
	}
}

bool CheckIsPowOf2(int n)
{
	if (n <= 0)
		return false;

	return ((n & (n - 1)) == 0);
}

bool FFT(Complex *input, Complex *output, int count)
{
	if (!CheckIsPowOf2(count))
	{
		DFT(input, output, count);
		return true;
	}

	Complex *workingComplexArray = output;
	memcpy(workingComplexArray, input, sizeof(Complex) * count);

	PermutationComplex(workingComplexArray, count);

	int offset = 0;
	int gap = 2;
	while (gap <= count)
	{
		int posBlock = 0;
		while (posBlock < count)
		{
			int halfGap = gap / 2;

			Complex *ptrComplexUpper = workingComplexArray + posBlock;
			Complex *ptrComplexLower = ptrComplexUpper + halfGap;

			int countInBlock = 0;
			while (countInBlock < halfGap)
			{
				Complex upper = *ptrComplexUpper;
				Complex lower = *ptrComplexLower;

				int k = countInBlock;

				Complex multiplier = Complex_Exp(-((2.0f * ((float)M_PI) / gap) * k));
				Complex lowerMultiplied = Complex_Multiply(lower, multiplier);

				Complex destUpper = Complex_Add(upper, lowerMultiplied);
				Complex destLower = Complex_Subtract(upper, lowerMultiplied);

				*ptrComplexUpper = destUpper;
				*ptrComplexLower = destLower;

				ptrComplexUpper++;
				ptrComplexLower++;

				countInBlock++;
			}

			posBlock += gap;
		}

		gap *= 2;
	}

	return true;
}

bool IFFT(Complex *input, Complex *output, int count)
{
	if (!CheckIsPowOf2(count))
	{
		IDFT(input, output, count);
		return true;
	}

	Complex *tempComplexArray = malloc(sizeof(Complex) * count);
	if (tempComplexArray == NULL)
		return false;

	Complex *ptrConjugate;

	Complex *ptrInput = input;
	ptrConjugate = tempComplexArray;
	for (int i = 0; i < count; i++)
	{
		*ptrConjugate = Complex_Conjugate(*ptrInput);

		ptrInput++;
		ptrConjugate++;
	}

	Complex *workingArray = output;

	FFT(tempComplexArray, workingArray, count);

	ptrConjugate = workingArray;
	for (int i = 0; i < count; i++)
	{
		*ptrConjugate = Complex_Conjugate(*ptrConjugate);
		*ptrConjugate = Complex_Divide(
			*ptrConjugate,
			Complex_NewReal(count)
		);

		ptrConjugate++;
	}

	free(tempComplexArray);

	return true;
}
