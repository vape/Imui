using System;

namespace Imui.Utility
{
    public struct ImCircularBuffer<T>
    {
        public readonly int Capacity;

        public int Head;
        public int Count;
        public T[] Array;

        public ImCircularBuffer(int capacity)
        {
            Capacity = capacity;

            Head = 0;
            Count = 0;
            Array = new T[capacity];
        }

        public ImCircularBuffer(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            Capacity = array.Length;

            Head = 0;
            Count = 0;
            Array = array;
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

        public bool TryPopBack(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            value = Array[Head];
            Head = (Head + 1) % Capacity;
            Count--;
            return true;
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
        
        public bool TryPeekFront(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            value = Array[(Head + Count - 1) % Capacity];
            return true;
        }

        public bool TryPopFront(out T value)
        {
            if (Count == 0)
            {
                value = default;
                return false;
            }

            value = Array[(Head + Count - 1) % Capacity];
            Count--;
            return true;
        }
    }
}