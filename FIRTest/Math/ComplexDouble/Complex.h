#pragma once

typedef struct
{
    double real;
    double imag;
}
Complex;

Complex Complex_Add(Complex a, Complex b);
Complex Complex_Subtract(Complex a, Complex b);
Complex Complex_Multiply(Complex a, Complex b);
Complex Complex_Divide(Complex a, Complex b);

double Complex_Abs(Complex a);
double Complex_Angle(Complex a);

Complex Complex_Exp(double angle);
Complex Complex_Pow(Complex a, double b);

Complex Complex_New(double real, double imag);
Complex Complex_NewReal(double real);
Complex Complex_NewImag(double imag);
Complex Complex_Zero();
