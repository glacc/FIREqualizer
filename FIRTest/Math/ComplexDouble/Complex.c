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
    double absSquareB = b.real * b.real + b.imag * b.imag;
    Complex result = {
        .real = (a.real * b.real + a.imag * b.imag) / absSquareB,
        .imag = (a.imag * b.real - a.real * b.imag) / absSquareB
    };
    return result;
}

double Complex_Abs(Complex a)
{
    return sqrt(a.real * a.real + a.imag * a.imag);
}

double Complex_Angle(Complex a)
{
    return atan2(a.real, a.imag);
}

Complex Complex_Exp(double angle)
{
    Complex result =
    {
        .real = cos(angle),
        .imag = sin(angle)
    };
    return result;
}

Complex Complex_Pow(Complex a, double b)
{
    return Complex_Exp(Complex_Angle(a) * b);
}

Complex Complex_New(double real, double imag)
{
    Complex result =
    {
        .real = real,
        .imag = imag
    };
    return result;
}

Complex Complex_NewReal(double real)
{
    Complex result =
    {
        .real = real,
        .imag = 0.0
    };
    return result;
}

Complex Complex_NewImag(double imag)
{
    Complex result =
    {
        .real = 0.0,
        .imag = imag
    };
    return result;
}

Complex Complex_Zero()
{
    Complex result =
    {
        .real = 0.0,
        .imag = 0.0
    };
    return result;
}
