using System;

namespace Imui.Utility
{
    internal struct DynamicArray<T>
    {
        public int Count;
        public T[] Array;
        
        public DynamicArray(int capacity)
        {
            Array = new T[capacity];
            Count = 0;
        }
        
        public bool RemoveAtUnordered(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException($"{index} out of range, count: {Count}");
            }

            Array[index] = Array[Count - 1];
            Count--;
            return true;
        }
        
        public void Add(T value)
        {
            EnsureCapacity(1);
            Array[Count++] = value; 
        }

        public void Add(ref T value)
        {
            EnsureCapacity(1);
            Array[Count++] = value; 
        }
        
        public void Push(T value)
        {
            EnsureCapacity(1);
            Array[Count++] = value;
        }

        public T Pop()
        {
            ImuiAssert.True(Count >= 0, "Popping empty array");
            
            return Array[--Count];
        }
        
        public ref T Peek()
        {
            ImuiAssert.True(Count >= 0, "Peeking empty array");
            
            return ref Array[Count - 1];
        }

        public void Clear(bool zero)
        {
            if (zero)
            {
                for (int i = 0; i < Count; ++i)
                {
                    Array[i] = default;
                }
            }

            Count = 0;
        }

        private void EnsureCapacity(int newElementsCount)
        {
            var requiredSize = Array.Length + newElementsCount;
            var ratio = 1;

            while ((Array.Length * ratio) < requiredSize)
            {
                ratio++;
            }

            Grow(ratio);
        }
        
        private void Grow(int ratio)
        {
            System.Array.Resize(ref Array, Array.Length * ratio);
        }
    }
}