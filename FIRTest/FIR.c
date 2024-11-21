#include "FIR.h"

#include "Collections/Vector.h"
#include "Math/Complex.h"
#include "DFT.h"

#define _USE_MATH_DEFINES
#include <malloc.h>
#include <memory.h>
#include <stdbool.h>
#include <math.h>

void FIR_CreateImpulse(float *freqs, float *impulse, int count)
{
    Vector freqsComplex;
    Vector_Zero(&freqsComplex);
    Vector_New(&freqsComplex, sizeof(Complex));

    for (int i = 0; i < count; i++)
    {
        Complex freqComplex = Complex_Multiply
        (
            Complex_Exp(2.0f * (float)M_PI * i / count),
            Complex_NewReal(freqs[i])
        );
        Vector_Add(&freqsComplex, &freqComplex);
    }

    Vector impulseComplex;
    Vector_Zero(&impulseComplex);
    Vector_New(&impulseComplex, sizeof(Complex));

    Vector_ClearAndFill(&impulseComplex, &(Complex[]){ Complex_Zero() }, count);

    IDFT(freqsComplex.array, impulseComplex.array, count);

    int i;
    int j = 0;

    i = count / 2;
    for (;i < count; i++)
        impulse[j++] = (*(Complex *)Vector_Index(&impulseComplex, i)).real;
    i = 0;
    for (;i < count / 2; i++)
        impulse[j++] = (*(Complex *)Vector_Index(&impulseComplex, i)).real;

    for (int i = 0; i < count; i++)
    {
        float window = 0.54f - 0.46f * cosf(2.0f * (float)M_PI * i / (count - 1));
        impulse[i] *= window;
    }
}

void FIR_ZeroFilter(FIR_Filter *filter)
{
    if (filter == NULL)
        return;

    memset(filter, 0, sizeof(FIR_Filter));
}

bool FIR_CreateFilterByFreqs(FIR_Filter *filter, float *freqs, int count)
{
    if (filter == NULL)
        return false;

    int filterSize = count * sizeof(float);
    filter->freqs = malloc(filterSize);
    filter->impulse = malloc(filterSize);
    if (filter->freqs == NULL || filter->impulse == NULL)
        goto FIR_CreateFilterByFreqs_AllocFail;

    memcpy(filter->freqs, freqs, filterSize);
    FIR_CreateImpulse(filter->freqs, filter->impulse, count);
    filter->impulseLength = count;

    int signalLength = count + 1;
    filter->originalSignal = calloc(signalLength, sizeof(float));
    filter->outputSignal = calloc(signalLength, sizeof(float));
    if (filter->originalSignal == NULL || filter->outputSignal == NULL)
        goto FIR_CreateFilterByFreqs_AllocFail;

    filter->signalLength = signalLength;

    return true;

FIR_CreateFilterByFreqs_AllocFail:

    FIR_DestroyFilter(filter);

    return false;
}

void FIR_UpdateFreqs(FIR_Filter *filter, float *freqs)
{
    if (filter == NULL || freqs == NULL)
        return;

    int count = filter->impulseLength;
    memcpy(filter->freqs, freqs, sizeof(float) * count);
    FIR_CreateImpulse(filter->freqs, filter->impulse, count);
}

void FIR_DestroyFilter(FIR_Filter *filter)
{
    if (filter == NULL)
        return;

    if (filter->freqs != NULL)
        free(filter->freqs);
    if (filter->impulse != NULL)
        free(filter->impulse);
    if (filter->originalSignal != NULL)
        free(filter->originalSignal);
    if (filter->outputSignal != NULL)
        free(filter->outputSignal);
}

float FIR_Next(FIR_Filter *filter, float input)
{
    int impulseLength = filter->impulseLength;
    int halfImpluseLength = impulseLength / 2;
    int signalLength = filter->signalLength;

    float *impulse = filter->impulse;
    float *originalSignal = filter->originalSignal;
    float *outputSignal = filter->outputSignal;

    float toReturn = outputSignal[0];

    float *ptrImpulse = impulse;
    float *ptrOriginalSignal = originalSignal;
    float *ptrOutputSignal = outputSignal;
    for (int i = 0; i < impulseLength; i++)
    {
        *ptrOutputSignal = (*(ptrOutputSignal + 1)) + ((*ptrImpulse) * input);
        *ptrOriginalSignal = *(ptrOriginalSignal + 1);

        ptrOutputSignal++;
        ptrOriginalSignal++;
        ptrImpulse++;
    }
    originalSignal[halfImpluseLength - 1] = input;

    outputSignal[signalLength - 1] = 0.0f;
    originalSignal[signalLength - 1] = 0.0f;

    return toReturn;
}
