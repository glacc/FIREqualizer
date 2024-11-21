#pragma once

#include "Math/Complex.h"

#include <stdbool.h>

bool FFT(Complex *input, Complex *output, int count);
bool IFFT(Complex *input, Complex *output, int count);
