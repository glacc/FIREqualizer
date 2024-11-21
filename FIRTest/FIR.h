#pragma once

#include <stdbool.h>
#include "Math/Complex.h"

typedef struct
{
    float *freqs;
    float *impulse;
    int impulseLength;
    
    float *originalSignal;
    float *outputSignal;
    int signalLength;
}
FIR_Filter;

void FIR_CreateImpulse(float *freqs, float *impulse, int count);

void FIR_ZeroFilter(FIR_Filter *filter);
bool FIR_CreateFilterByFreqs(FIR_Filter *filter, float *freqs, int count);
void FIR_UpdateFreqs(FIR_Filter *filter, float *freqs);
void FIR_DestroyFilter(FIR_Filter *filter);

float FIR_Next(FIR_Filter *filter, float input);
