#include "FIR.h"

#include "FIR_DllExport.h"

void __cdecl Extern_FIR_CreateImpulse(float *freqs, float *impulse, int count)
{
    FIR_CreateImpulse(freqs, impulse, count);
}

void __cdecl Extern_FIR_ZeroFilter(FIR_Filter *filter)
{
    FIR_ZeroFilter(filter);
}

bool __cdecl Extern_FIR_CreateFilterByFreqs(FIR_Filter *filter, float *freqs, int count)
{
    return FIR_CreateFilterByFreqs(filter, freqs, count);
}

void __cdecl Extern_FIR_UpdateFreqs(FIR_Filter *filter, float *freqs)
{
    FIR_UpdateFreqs(filter, freqs);
}

void __cdecl Extern_FIR_DestroyFilter(FIR_Filter *filter)
{
    FIR_DestroyFilter(filter);
}

float __cdecl Extern_FIR_Next(FIR_Filter *filter, float input)
{
    return FIR_Next(filter, input);
}
