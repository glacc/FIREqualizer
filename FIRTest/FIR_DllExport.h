#pragma once

#include "FIR.h"

#include <stdbool.h>

extern __declspec(dllexport) void __cdecl Extern_FIR_CreateImpulse(float *freqs, float *impulse, int count);

extern __declspec(dllexport) void __cdecl Extern_FIR_ZeroFilter(FIR_Filter *filter);
extern __declspec(dllexport) bool __cdecl Extern_FIR_CreateFilterByFreqs(FIR_Filter *filter, float *freqs, int count);
extern __declspec(dllexport) void __cdecl Extern_FIR_UpdateFreqs(FIR_Filter *filter, float *freqs);
extern __declspec(dllexport) void __cdecl Extern_FIR_DestroyFilter(FIR_Filter *filter);

extern __declspec(dllexport) float __cdecl Extern_FIR_Next(FIR_Filter *filter, float input);
