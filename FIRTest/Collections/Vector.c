#include "Vector.h"

#include <memory.h>

static int Vector_defaultInitSize = 128;

void Vector_Zero(Vector *vector)
{
    memset(vector, 0, sizeof(Vector));
}

bool Vector_New(Vector *vector, int sizeOfElem)
{
    vector->array = malloc(sizeOfElem * Vector_defaultInitSize);
    if (vector->array == NULL)
        return false;

    vector->sizePerElem = sizeOfElem;
    vector->size = Vector_defaultInitSize;
    vector->count = 0;

    return true;
}

void Vector_Dispose(Vector *vector)
{
    if (vector->array == NULL)
        return;

    free(vector->array);
    vector->array = NULL;
}

bool Vector_Resize(Vector *vector, int newSize)
{
    if (newSize < vector->count)
        return false;

    void *newArrayPtr = realloc(vector->array, vector->sizePerElem * newSize);
    if (newArrayPtr == NULL)
        return false;

    vector->array = newArrayPtr;

    vector->size = newSize;

    return true;
}

bool Vector_Add(Vector *vector, void *elem)
{
    if (vector->count == vector->size)
    {
        if (!Vector_Resize(vector, vector->size * 2))
            return false;
    }

    int offset = vector->count * vector->sizePerElem;
    memcpy((int8_t *)vector->array + offset, elem, vector->sizePerElem);

    vector->count++;

    return true;
}

bool Vector_InsertBefore(Vector *vector, int index, void *elem)
{
    if (vector->count == vector->size)
    {
        if (!Vector_Resize(vector, vector->size * 2))
            return false;
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

    return true;
}

void Vector_RemoveAt(Vector *vector, int index)
{
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
}

void Vector_Clear(Vector *vector)
{
    vector->count = 0;
}

bool Vector_ClearAndFill(Vector *vector, void *elem, int count)
{
    Vector_Clear(vector);

    for (int i = 0; i < count; i++)
    {
        if (!Vector_Add(vector, elem))
            return false;
    }

    return true;
}

void *Vector_Index(Vector *vector, int index)
{
    if (index >= vector->count || index < 0)
        return NULL;

    int offset = vector->sizePerElem * index;
    return (int8_t *)vector->array + offset;
}
