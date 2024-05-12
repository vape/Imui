using System;
using UnityEngine;

namespace Imui.Utility
{
    public struct ResizeableBuffer<T>
    {
        public T[] Array;

        public ResizeableBuffer(int capacity)
        {
            Array = new T[capacity];
        }
        
        public void Grow(int length)
        {
            if (Array.Length >= length)
            {
                return;
            }
            
            System.Array.Resize(ref Array, Mathf.NextPowerOfTwo(length));
        }

        public Span<T> AsSpan(int length)
        {
            if (length > Array.Length)
            {
                Grow(length);
            }
            
            return new Span<T>(Array, 0, length);
        }
        
        public static implicit operator Span<T>(ResizeableBuffer<T> buffer)
        {
            return buffer.Array;
        }
    }
}