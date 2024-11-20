#pragma once

#include <stdbool.h>
#include "Math/Complex.h"

typedef struct
{
    double *freqs;
    double *impulse;
    int impulseLength;
    
    // Queue based FIR filter.
    double *originalSignal;
    double *outputSignal;
    int signalLength;

    int pos;
}
FIR_Filter;

void FIR_CreateImpulse(double *freqs, double *impulse, int count);

void FIR_ZeroFilter(FIR_Filter *filter);
bool FIR_CreateFilterByFreqs(FIR_Filter *filter, double *freqs, int count);
void FIR_DestroyFilter(FIR_Filter *filter);

double FIR_Next(FIR_Filter *filter, double input);
