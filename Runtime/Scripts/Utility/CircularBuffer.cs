using System;

namespace Imui.Utility
{
    internal struct CircularBuffer<T>
    {
        public readonly int Capacity;

        public int Head;
        public int Count;
        public T[] Array;

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;

            Head = 0;
            Count = 0;
            Array = new T[capacity];
        }

        public ref T Get(int index)
        {
            return ref Array[(Head + index) % Capacity];
        }
        
        public void Set(int index, T value)
        {
            Array[(Head + index) % Capacity] = value;
        }
        
        public void Clear()
        {
            Head = 0;
            Count = 0;
        }

        public void PushBack(T value)
        {
            Head = (Head - 1) % Capacity;

            if (Head < 0)
            {
                Head += Capacity;
            }

            Array[Head] = value;

            if (Count < Capacity)
            {
                Count++;
            }
        }

        public void PopBack(out T value)
        {
            value = Array[Head];
            PopBack();
        }

        public void PopBack()
        {
            if (Count == 0)
            {
                return;
            }
            
            Head = (Head + 1) % Capacity;
            Count--;
        }

        public void PushFront(T value)
        {
            Array[(Head + Count) % Capacity] = value;

            if (Count == Capacity)
            {
                Head = (Head + 1) % Capacity;
            }
            else
            {
                Count++;
            }
        }

        public void PopFront(out T value)
        {
            if (Count > 0)
            {
                value = Array[(Head + Count - 1) % Capacity];
                Count--;
            }
            else
            {
                value = default;
            }
        }

        public void PopFront()
        {
            if (Count > 0)
            {
                Count--;
            }
        }
    }
}