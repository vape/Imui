using System;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    public unsafe class ImArena
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
        
        public ref T Alloc<T>() where T : unmanaged
        {
            return ref *(T*)Reserve(sizeof(T), false);
        }
        
        public T* AllocPtr<T>() where T : unmanaged
        {
            return (T*)Reserve(sizeof(T), false);
        }
        
        public Span<T> AllocArray<T>(int length) where T : unmanaged
        {
            return new Span<T>(Reserve(sizeof(T) * length, false), length);
        }

        public void ReallocArray<T>(ref Span<T> array, int length) where T : unmanaged
        {
            fixed (void* ptr = array)
            {
                var arraySize = Align(sizeof(T) * array.Length);
                var bufferTail = (void*)(buffer + size);
                
                if ((byte*)ptr + arraySize == bufferTail)
                {
                    size -= arraySize;
                }
            }

            var newArray = AllocArray<T>(length);
            
            fixed (void* oldPtr = array)
            fixed (void* newPtr = newArray)
            {
                if (oldPtr != newPtr)
                {
                    UnsafeUtility.MemCpy(newPtr, oldPtr, sizeof(T) * array.Length);
                }
            }

            array = newArray;
        }
        
        public void Clear()
        {
            while (prevBuffers.TryPop(out var ptr))
            {
                // TODO (artem-s): linked list with all of the chunks maybe
                Marshal.FreeHGlobal(ptr);
            }

            size = 0;
        }

        private void* Reserve(int bytes, bool zero)
        {
            bytes = Align(bytes);
            
            if (size + bytes > capacity)
            {
                GrowToFit(size + bytes);
            }
            
            var ptr = (void*)(buffer + size);
            if (zero)
            {
                UnsafeUtility.MemSet(ptr, 0, bytes);
            }
            size += bytes;
            
            return ptr;
        }

        private void GrowToFit(int requiredCapacity)
        {
            var newCapacity = capacity * 2;
            
            while (newCapacity < requiredCapacity)
            {
                newCapacity *= 2;
            }
            
            prevBuffers.Push(buffer);
            buffer = Marshal.AllocHGlobal(newCapacity);
            capacity = requiredCapacity;
        }
        
        private static int Align(int size)
        {
            return (sizeof(IntPtr) * ((size + sizeof(IntPtr) - 1) / sizeof(IntPtr)));
        }
    }
}