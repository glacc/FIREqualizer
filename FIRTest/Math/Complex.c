#include "Complex.h"
#include <math.h>

Complex Complex_Add(Complex a, Complex b)
{
    Complex result = {
        .real = a.real + b.real,
        .imag = a.imag + b.imag
    };
    return result;
}

Complex Complex_Subtract(Complex a, Complex b)
{
    Complex result = {
        .real = a.real - b.real,
        .imag = a.imag - b.imag
    };
    return result;
}

Complex Complex_Multiply(Complex a, Complex b)
{
    Complex result = {
        .real = a.real * b.real - a.imag * b.imag,
        .imag = a.real * b.imag + a.imag * b.real
    };
    return result;
}

Complex Complex_Divide(Complex a, Complex b)
{
    float absSquareB = b.real * b.real + b.imag * b.imag;
    Complex result = {
        .real = (a.real * b.real + a.imag * b.imag) / absSquareB,
        .imag = (a.imag * b.real - a.real * b.imag) / absSquareB
    };
    return result;
}

float Complex_Abs(Complex a)
{
    return sqrtf(a.real * a.real + a.imag * a.imag);
}

float Complex_Angle(Complex a)
{
    return atan2f(a.real, a.imag);
}

Complex Complex_Exp(float angle)
{
    Complex result =
    {
        .real = cosf(angle),
        .imag = sinf(angle)
    };
    return result;
}

Complex Complex_Pow(Complex a, float b)
{
    return Complex_Exp(Complex_Angle(a) * b);
}

Complex Complex_New(float real, float imag)
{
    Complex result =
    {
        .real = real,
        .imag = imag
    };
    return result;
}

Complex Complex_NewReal(float real)
{
    Complex result =
    {
        .real = real,
        .imag = 0.0f
    };
    return result;
}

Complex Complex_NewImag(float imag)
{
    Complex result =
    {
        .real = 0.0f,
        .imag = imag
    };
    return result;
}

Complex Complex_Zero()
{
    Complex result =
    {
        .real = 0.0f,
        .imag = 0.0f
    };
    return result;
}
