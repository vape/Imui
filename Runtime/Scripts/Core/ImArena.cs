using System;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    public class ImArena
    {
        public int Capacity => capacity;
        public int Size => size;
        
        private ImDynamicArray<IntPtr> prevBuffers;
        private IntPtr buffer;
        private int capacity;
        private int size;

        public ImArena(int capacity)
        {
            if (capacity <= 0)
            {
                throw new InvalidOperationException("Capacity must be more then zero");
            }
            
            this.capacity = capacity;
            
            buffer = Marshal.AllocHGlobal(capacity);
            prevBuffers = new ImDynamicArray<IntPtr>(4);
        }
        
        public unsafe ref T Alloc<T>() where T : unmanaged
        {
            var typeSize = sizeof(T);
            if (size + typeSize > capacity)
            {
                GrowToFit(size + typeSize);
            }
            
            return ref *(T*)Reserve(typeSize, false);
        }
        
        public unsafe Span<T> AllocArray<T>(int length) where T : unmanaged
        {
            var arraySize = sizeof(T) * length;
            if (size + arraySize > capacity)
            {
                GrowToFit(size + arraySize);
            }

            return new Span<T>(Reserve(arraySize, true), length);
        }

        public unsafe void ResizeArray<T>(ref Span<T> array, int length) where T : unmanaged
        {
            var currArraySize = array.Length * sizeof(T);
            var nextArraySize = length * sizeof(T);
            var arraySizeDelta = nextArraySize - currArraySize;
            
            fixed (void* currArrayPtr = array)
            {
                var currArrayTail = (byte*)currArrayPtr + currArraySize;
                var bufferTail = (byte*)buffer + size;
                
                if (currArrayTail == bufferTail && capacity > (size + arraySizeDelta))
                {
                    Reserve(arraySizeDelta, true);
                    array = new Span<T>(currArrayPtr, nextArraySize);
                    return;
                }
            }

            var nextArray = AllocArray<T>(length);

            fixed (void* nextArrayPtr = nextArray)
            fixed (void* currArrayPtr = array)
            {
                UnsafeUtility.MemCpy(nextArrayPtr, currArrayPtr, array.Length);
            }
            
            array = nextArray;
        }
        
        public void Clear()
        {
            while (prevBuffers.TryPop(out var ptr))
            {
                Marshal.FreeHGlobal(ptr);
            }

            size = 0;
        }

        private unsafe void* Reserve(int byteSize, bool zero)
        {
            var ptr = (void*)(buffer + size);
            if (zero)
            {
                UnsafeUtility.MemSet(ptr, 0, byteSize);
            }
            size += byteSize;
            
            return ptr;
        }

        private void GrowToFit(int requiredCapacity)
        {
            var newCapacity = capacity * 2;
            
            while (newCapacity < requiredCapacity)
            {
                newCapacity *= 2;
            }
            
            prevBuffers.Push(buffer); // (artem-s): keep exiting buffer until end of the frame
            buffer = Marshal.AllocHGlobal(newCapacity);
            capacity = requiredCapacity;
        }
    }
}