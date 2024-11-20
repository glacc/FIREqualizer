#include "Vector.h"

#include <memory.h>

static int Vector_defaultInitSize = 128;

Vector_Status Vector_Zero(Vector *vector)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    memset(vector, 0, sizeof(Vector));

    return Vector_Status_Success;
}

Vector_Status Vector_New(Vector *vector, int sizeOfElem)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array != NULL)
        return Vector_Status_AlreadyAllocated;

    vector->array = malloc(sizeOfElem * Vector_defaultInitSize);
    if (vector->array == NULL)
        return Vector_Status_UnableToAllocate;

    vector->sizePerElem = sizeOfElem;
    vector->size = Vector_defaultInitSize;
    vector->count = 0;

    return Vector_Status_Success;
}

Vector_Status Vector_Dispose(Vector *vector)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array == NULL)
        return Vector_Status_IsUnallocated;

    free(vector->array);
    vector->array = NULL;

    return Vector_Status_Success;
}

Vector_Status Vector_Resize(Vector *vector, int newSize)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array == NULL)
        return Vector_Status_IsUnallocated;

    if (newSize < vector->count)
        return Vector_Status_SizeIsSmallerThanElemCount;

    /*
    int originalContentSizeInBytes = vector->sizePerElem * vector->count;

    void *newArrayPtr = malloc(vector->sizePerElem * newSize);
    if (newArrayPtr == NULL)
        return Vector_Status_UnableToAllocate;

    memcpy(newArrayPtr, vector->array, originalContentSizeInBytes);

    free(vector->array);
    */

    void *newArrayPtr = realloc(vector->array, vector->sizePerElem * newSize);
    if (newArrayPtr == NULL)
        return Vector_Status_UnableToAllocate;

    vector->array = newArrayPtr;

    vector->size = newSize;

    return Vector_Status_Success;
}

Vector_Status Vector_Add(Vector *vector, void *elem)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array == NULL)
        return Vector_Status_IsUnallocated;

    if (vector->count == vector->size)
    {
        Vector_Status status = Vector_Resize(vector, vector->size * 2);
        if (status != Vector_Status_Success)
            return status;
    }

    int offset = vector->count * vector->sizePerElem;
    memcpy((int8_t *)vector->array + offset, elem, vector->sizePerElem);

    vector->count++;

    return Vector_Status_Success;
}

Vector_Status Vector_InsertBefore(Vector *vector, int index, void *elem)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array == NULL)
        return Vector_Status_IsUnallocated;

    if (index >= vector->count)
        return Vector_Status_OutOfRange;

    if (vector->count == vector->size)
    {
        Vector_Status status = Vector_Resize(vector, vector->size * 2);
        if (status != Vector_Status_Success)
            return status;
    }

    int8_t *ptrCurrElem = (int8_t *)vector->array + vector->count * vector->sizePerElem;
    int8_t *ptrPrevElem = ptrCurrElem - vector->sizePerElem;

    int moveCount = vector->count - index;
    for (int i = 0; i < moveCount; i++)
    {
        memcpy(ptrCurrElem, ptrPrevElem, vector->sizePerElem);

        ptrCurrElem = ptrPrevElem;
        ptrPrevElem -= vector->sizePerElem;
    }
    memcpy(ptrCurrElem, elem, vector->sizePerElem);

    vector->count++;

    return Vector_Status_Success;
}

Vector_Status Vector_RemoveAt(Vector *vector, int index)
{
    if (vector == NULL)
        return Vector_Status_Nullptr;

    if (vector->array == NULL)
        return Vector_Status_IsUnallocated;

    if (index >= vector->count)
        return Vector_Status_OutOfRange;

    int8_t *ptrCurrElem = (int8_t *)vector->array + index * vector->sizePerElem;
    int8_t *ptrNextElem = ptrCurrElem + vector->sizePerElem;

    int moveCount = vector->count - index - 1;
    for (int i = 0; i < moveCount; i++)
    {
        memcpy(ptrCurrElem, ptrNextElem, vector->sizePerElem);

        ptrCurrElem = ptrNextElem;
        ptrNextElem += vector->sizePerElem;
    }

    vector->count--;

    return Vector_Status_Success;
}
