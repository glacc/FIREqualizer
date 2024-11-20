#pragma once

#include <stdlib.h>
#include <stdint.h>

extern int Vector_defaultInitSize;

typedef struct
{
    void *array;
    int sizePerElem;
    int size;
    int count;
}
Vector;

typedef enum
{
    Vector_Status_Success,

    Vector_Status_Nullptr,

    Vector_Status_UnableToAllocate,
    Vector_Status_AlreadyAllocated,
    Vector_Status_IsUnallocated,

    Vector_Status_SizeIsSmallerThanElemCount,

    Vector_Status_OutOfRange
}
Vector_Status;

Vector_Status Vector_Zero(Vector *vector);

Vector_Status Vector_New(Vector *vector, int sizeOfElem);
Vector_Status Vector_Dispose(Vector *vector);

Vector_Status Vector_Resize(Vector *vector, int newSize);

Vector_Status Vector_Add(Vector *vector, void *elem);
Vector_Status Vector_InsertBefore(Vector *vector, int index, void *elem);
Vector_Status Vector_RemoveAt(Vector *vector, int index);
