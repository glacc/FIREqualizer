#pragma once

#include "FIR.h"

#include <stdbool.h>

extern __declspec(dllexport) void __cdecl Extern_FIR_CreateImpulse(double *freqs, double *impulse, int count);

extern __declspec(dllexport) void __cdecl Extern_FIR_ZeroFilter(FIR_Filter *filter);
extern __declspec(dllexport) bool __cdecl Extern_FIR_CreateFilterByFreqs(FIR_Filter *filter, double *freqs, int count);
extern __declspec(dllexport) void __cdecl Extern_FIR_DestroyFilter(FIR_Filter *filter);

extern __declspec(dllexport) double __cdecl Extern_FIR_Next(FIR_Filter *filter, double input);
