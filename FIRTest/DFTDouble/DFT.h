#pragma once

#include "Math/Complex.h"

void DFT(Complex *input, Complex *output, int count);
void IDFT(Complex *input, Complex *output, int count);
