#pragma once

#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>

extern int Vector_defaultInitSize;

typedef struct
{
    void *array;
    int sizePerElem;
    int size;
    int count;
}
Vector;

void Vector_Zero(Vector *vector);

bool Vector_New(Vector *vector, int sizeOfElem);
void Vector_Dispose(Vector *vector);

bool Vector_Resize(Vector *vector, int newSize);

bool Vector_Add(Vector *vector, void *elem);
bool Vector_InsertBefore(Vector *vector, int index, void *elem);
void Vector_RemoveAt(Vector *vector, int index);
void Vector_Clear(Vector *vector);
bool Vector_ClearAndFill(Vector *vector, void *elem, int count);

void *Vector_Index(Vector *vector, int index);
