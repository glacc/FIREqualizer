#include <stdio.h>
#include <math.h>

#include "Collections/Vector.h"
#include "Math/Complex.h"
#include "DFT.h"
#include "FIR.h"

void TestDFTAndIDFT()
{
    Vector complexOriginal;
    Vector complexDFT;
    Vector complexIDFT;
    Vector_Zero(&complexOriginal);
    Vector_Zero(&complexDFT);
    Vector_Zero(&complexIDFT);
    Vector_New(&complexOriginal, sizeof(Complex));
    Vector_New(&complexDFT, sizeof(Complex));
    Vector_New(&complexIDFT, sizeof(Complex));

    for (int i = 0;;)
    {
        while (i < 4)
        {
            Vector_Add(&complexOriginal, &(Complex[]){ Complex_NewReal(1.0) });
            i++;
        }

        while (i < 8)
        {
            Vector_Add(&complexOriginal, &(Complex[]){ Complex_Zero() });
            i++;
        }

        break;
    }
    for (int i = 0; i < 8; i++)
    {
        Vector_Add(&complexDFT, &(Complex[]){ Complex_Zero() });
        Vector_Add(&complexIDFT, &(Complex[]){ Complex_Zero() });
    }

    // Original
    printf("Original:\n");
    for (int i = 0; i < 8; i++)
        printf("%.1lf ", Complex_Abs(((Complex *)complexOriginal.array)[i]));
    printf("\n");

    // DFT
    DFT(complexOriginal.array, complexDFT.array, 8);
    printf("DFT:\n");
    for (int i = 0; i < 8; i++)
        printf("%.3lf ", Complex_Abs(((Complex *)complexDFT.array)[i]));
    printf("\n");

    // IDFT
    IDFT(complexDFT.array, complexIDFT.array, 8);
    printf("IDFT:\n");
    for (int i = 0; i < 8; i++)
        printf("%.3lf ", Complex_Abs(((Complex *)complexIDFT.array)[i]));
    printf("\n");

    printf("\n");

    Vector_Dispose(&complexOriginal);
    Vector_Dispose(&complexDFT);
    Vector_Dispose(&complexIDFT);
}

void TestFIR()
{
    const int count = 32;
    const int cutoff = 8;

    Vector freqs;
    Vector impulse;
    Vector_Zero(&freqs);
    Vector_Zero(&impulse);
    Vector_New(&freqs, sizeof(double));
    Vector_New(&impulse, sizeof(double));

    Vector_ClearAndFill(&freqs, &(double){0.0}, count);
    for (int i = 0; i < cutoff; i++)
    {
        *(double *)Vector_Index(&freqs, i) = 1.0;
        *(double *)Vector_Index(&freqs, freqs.count - 1 - i) = 1.0;
    }

    Vector_ClearAndFill(&impulse, &(double){0.0}, count);

    FIR(freqs.array, impulse.array, count);

    printf("Freqs:\n");
    for (int i = 0; i < freqs.count; i++)
        printf("%.1lf ", *(double *)Vector_Index(&freqs, i));
    printf("\n");

    printf("Impulse:\n");
    for (int i = 0; i < impulse.count; i++)
        printf("%.3lf ", *(double *)Vector_Index(&impulse, i));
    printf("\n");
}

int main()
{
    TestFIR();

    return 0;
}
