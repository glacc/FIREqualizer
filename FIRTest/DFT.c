#include "DFT.h"

#define _USE_MATH_DEFINES
#include <math.h>

void DFT(Complex *input, Complex *output, int count)
{
    for (int i = 0; i < count; i++)
        output[i] = Complex_Zero();
    
    for (int k = 0; k < count; k++)
    {
        for (int n = 0; n < count; n++)
        {
            output[k] = Complex_Add
            (
                output[k],
                Complex_Multiply
                (
                    input[n],
                    Complex_Exp(-((2.0f * (float)M_PI / count) * (k * n)))
                )
            );
        }
    }
}

void IDFT(Complex *input, Complex *output, int count)
{
    for (int i = 0; i < count; i++)
        output[i] = Complex_Zero();

    for (int k = 0; k < count; k++)
    {
        for (int n = 0; n < count; n++)
        {
            output[k] = Complex_Add
            (
                output[k],
                Complex_Divide(
                    Complex_Multiply
                    (
                        input[n],
                        Complex_Exp((2.0f * (float)M_PI / count) * (k * n))
                    ),
                    Complex_NewReal((float)count)
                )
            );
        }
    }
}
