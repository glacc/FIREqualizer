#pragma once

typedef struct
{
    float real;
    float imag;
}
Complex;

Complex Complex_Add(Complex a, Complex b);
Complex Complex_Subtract(Complex a, Complex b);
Complex Complex_Multiply(Complex a, Complex b);
Complex Complex_Divide(Complex a, Complex b);

float Complex_Abs(Complex a);
float Complex_Angle(Complex a);

Complex Complex_Exp(float angle);
Complex Complex_Pow(Complex a, float b);

Complex Complex_New(float real, float imag);
Complex Complex_NewReal(float real);
Complex Complex_NewImag(float imag);
Complex Complex_Zero();
