using System;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    /// <summary>
    /// Arena allocator
    /// </summary>
    public unsafe class ImArena
    {
        /// <summary>
        /// Total system memory currently allocated 
        /// </summary>
        public int Capacity => capacity;
        /// <summary>
        /// Used portion of allocated memory
        /// </summary>
        public int Size => size;
        
        private ImDynamicArray<IntPtr> prevBuffers;
        private IntPtr buffer;
        private int capacity;
        private int size;

        /// <summary>
        /// Initializes a new instance of the ImArena class with the specified capacity.
        /// Allocates a buffer of the given size.
        /// </summary>
        /// <param name="capacity">Initial capacity for the allocator</param>
        /// <exception cref="InvalidOperationException">Thrown if capacity is less than or equal to zero</exception>
        public ImArena(int capacity)
        {
            if (capacity <= 0)
            {
                throw new InvalidOperationException("Capacity must be more than zero");
            }
            
            this.capacity = capacity;
            
            buffer = Marshal.AllocHGlobal(capacity);
            prevBuffers = new ImDynamicArray<IntPtr>(4);
        }
        
        /// <summary>
        /// Allocates an object of type T. Does not zero memory.
        /// </summary>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Reference to the allocated object</returns>
        public ref T Alloc<T>() where T : unmanaged
        {
            return ref *(T*)Reserve(sizeof(T), false);
        }
        
        /// <summary>
        /// Allocates an object of type T. Does not zero memory.
        /// </summary>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Pointer to the allocated object</returns>
        public T* AllocPtr<T>() where T : unmanaged
        {
            return (T*)Reserve(sizeof(T), false);
        }
        
        /// <summary>
        /// Allocates an array of objects of type T. Does not zero memory.
        /// </summary>
        /// <param name="length">Number of elements in the array</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Span of type T with the given length</returns>
        public Span<T> AllocArray<T>(int length) where T : unmanaged
        {
            return new Span<T>(Reserve(sizeof(T) * length, false), length);
        }

        /// <summary>
        /// Reallocates an array of objects of type T. If possible, reuses the same memory segment.
        /// Copies the existing data to the new location if necessary.
        /// </summary>
        /// <param name="array">Reference to the array</param>
        /// <param name="length">New length of the array</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
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
        
        /// <summary>
        /// Resets the allocator by setting the used memory size to zero.
        /// Frees all previously allocated chunks of memory.
        /// </summary>
        public void Clear()
        {
            while (prevBuffers.TryPop(out var ptr))
            {
                // TODO (artem-s): linked list with all of the chunks maybe
                Marshal.FreeHGlobal(ptr);
            }

            size = 0;
        }

        /// <summary>
        /// Reserves a block of memory of the specified size.
        /// If needed, grows the internal buffer.
        /// Optionally, zeroes out the allocated memory.
        /// </summary>
        /// <param name="bytes">Number of bytes to allocate</param>
        /// <param name="zero">Indicates whether to zero the memory</param>
        /// <returns>Pointer to the reserved memory</returns>
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

        /// <summary>
        /// Expands the internal buffer to accommodate the specified capacity.
        /// Allocates a new buffer and adjusts the capacity.
        /// </summary>
        /// <param name="requiredCapacity">Minimum capacity required</param>
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
        
        /// <summary>
        /// Aligns the given size to a multiple of the pointer size.
        /// </summary>
        /// <param name="size">Size to align</param>
        /// <returns>Aligned size</returns>
        private static int Align(int size)
        {
            return (sizeof(IntPtr) * ((size + sizeof(IntPtr) - 1) / sizeof(IntPtr)));
        }
    }
}
