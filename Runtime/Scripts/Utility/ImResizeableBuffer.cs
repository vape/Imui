using System;
using UnityEngine;

namespace Imui.Utility
{
    public struct ImResizeableBuffer<T>
    {
        public T[] Array;

        public ImResizeableBuffer(int capacity)
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
        
        public static implicit operator Span<T>(ImResizeableBuffer<T> buffer)
        {
            return buffer.Array;
        }
    }
}