#include "FIR.h"

#include "FIR_DllExport.h"

void __cdecl Extern_FIR_CreateImpulse(double *freqs, double *impulse, int count)
{
    FIR_CreateImpulse(freqs, impulse, count);
}

void __cdecl Extern_FIR_ZeroFilter(FIR_Filter *filter)
{
    FIR_ZeroFilter(filter);
}

bool __cdecl Extern_FIR_CreateFilterByFreqs(FIR_Filter *filter, double *freqs, int count)
{
    return FIR_CreateFilterByFreqs(filter, freqs, count);
}

void __cdecl Extern_FIR_DestroyFilter(FIR_Filter *filter)
{
    FIR_DestroyFilter(filter);
}

double __cdecl Extern_FIR_Next(FIR_Filter *filter, double input)
{
    return FIR_Next(filter, input);
}
