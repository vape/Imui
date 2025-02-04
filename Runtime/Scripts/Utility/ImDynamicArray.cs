using System;

namespace Imui.Utility
{
    internal struct ImDynamicArray<T>
    {
        public int Count;
        public T[] Array;
        
        public ImDynamicArray(int capacity)
        {
            Array = new T[capacity];
            Count = 0;
        }
        
        public bool RemoveAtFast(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException($"{index} out of range, count: {Count}");
            }

            Array[index] = Array[Count - 1];
            Count--;
            return true;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException($"{index} out of range, count: {Count}");
            }

            System.Array.Copy(Array, index + 1, Array, index, --Count - index);
        }
        
        public void Add(T value)
        {
            EnsureCapacity(Count + 1);
            Array[Count++] = value; 
        }
        
        public void Push(in T value)
        {
            EnsureCapacity(Count + 1);
            Array[Count++] = value;
        }

        public bool TryPop(out T value)
        {
            if (Count > 0)
            {
                value = Pop();
                return true;
            }

            value = default;
            return false;
        }
        
        public T Pop()
        {
            ImAssert.IsTrue(Count > 0, "Count > 0");
            
            return Array[--Count];
        }

        public T TryPeek(T @default = default)
        {
            if (Count == 0)
            {
                return @default;
            }

            return Peek();
        }
        
        public bool TryPeek(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            value = Peek();
            return true;
        }
        
        public ref T Peek()
        {
            ImAssert.IsTrue(Count > 0, "Count > 0");
            
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

        private void EnsureCapacity(int count)
        {
            if (count <= Array.Length)
            {
                return;
            }

            var newLength = Array.Length * 2;
            
            while (newLength < count)
            {
                newLength *= 2;
            }

            System.Array.Resize(ref Array, newLength);
        }
    }
}