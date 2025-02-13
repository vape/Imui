using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    /// <summary>
    /// Arena allocator
    /// </summary>
    public unsafe class ImArena: IDisposable
    {
        /// <summary>
        /// Total system memory currently allocated 
        /// </summary>
        public int Capacity
        {
            get
            {
                var sum = 0;

                for (int i = 0; i < blocks.Count; ++i)
                {
                    sum += blocks.Array[i].Capacity;
                }

                return sum;
            }
        }

        /// <summary>
        /// Used portion of allocated memory
        /// </summary>
        public int Size
        {
            get
            {
                var sum = 0;

                for (int i = 0; i < blocks.Count; ++i)
                {
                    sum += blocks.Array[i].Size;
                }

                return sum;
            }
        }

        private int lastBlock;
        private ImDynamicArray<MemoryBlock> blocks;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the ImArena class with the specified capacity.
        /// Allocates a buffer of the given size.
        /// </summary>
        /// <param name="capacity">Initial capacity for the allocator</param>
        public ImArena(int capacity)
        {
            ImAssert.IsTrue(capacity > 0, "capacity > 0");

            blocks = new ImDynamicArray<MemoryBlock>(4);
            blocks.Add(new MemoryBlock(capacity));
        }

        /// <summary>
        /// Allocates an object of type T. Does not zero memory.
        /// </summary>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Reference to the allocated object</returns>
        public ref T Alloc<T>() where T: unmanaged
        {
            return ref *(T*)Reserve(sizeof(T), false);
        }

        /// <summary>
        /// Allocates an object of type T. Does not zero memory.
        /// </summary>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Pointer to the allocated object</returns>
        public T* AllocUnsafe<T>() where T: unmanaged
        {
            return (T*)Reserve(sizeof(T), false);
        }

        /// <summary>
        /// Allocates an array of objects of type T. Does not zero memory.
        /// </summary>
        /// <param name="length">Number of elements in the array</param>
        /// <param name="zero">Whether to clear allocated memory</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Span of type T with the given length</returns>
        public Span<T> AllocArray<T>(int length, bool zero = false) where T: unmanaged
        {
            return new Span<T>(Reserve(sizeof(T) * length, zero), length);
        }

        /// <summary>
        /// Reallocates an array of objects of type T. If possible, reuses the same memory segment.
        /// Copies the existing data to the new location if necessary.
        /// </summary>
        /// <param name="array">Reference to the array</param>
        /// <param name="length">New length of the array</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Span of newly allocated array</returns>
        public Span<T> ReallocArray<T>(ref Span<T> array, int length) where T: unmanaged
        {
            fixed (T* ptr = array)
            {
                return new Span<T>(ReallocArrayUnsafe(ptr, array.Length, length), length);
            }
        }

        /// <summary>
        /// Allocates an array of objects of type T.
        /// </summary>
        /// <param name="length">Number of elements in the array</param>
        /// <param name="zero">Whether to clear allocated memory</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Pointer to array of type T with the given length</returns>
        public T* AllocArrayUnsafe<T>(int length, bool zero = false) where T: unmanaged
        {
            return (T*)Reserve(sizeof(T) * length, zero);
        }

        /// <summary>
        /// Reallocates an array of objects of type T. If possible, reuses the same memory segment.
        /// Copies the existing data to the new location if necessary.
        /// </summary>
        /// <param name="array">Pointer to the array</param>
        /// <param name="prevLength">Previous length of the array</param>
        /// <param name="newLength">New length of the array</param>
        /// <typeparam name="T">Built-in type or unmanaged struct</typeparam>
        /// <returns>Pointer to newly allocated array</returns>
        public T* ReallocArrayUnsafe<T>(T* array, int prevLength, int newLength) where T: unmanaged
        {
            var prevLengthAligned = Align(sizeof(T) * prevLength);
            var newLengthAligned = Align(sizeof(T) * newLength);

            if (lastBlock >= 0)
            {
                ref var block = ref blocks.Array[lastBlock];
                var tail = (void*)(block.Ptr + block.Size);

                if ((byte*)array + prevLengthAligned == tail &&
                    (block.Capacity - block.Size - prevLengthAligned) >= newLengthAligned)
                {
                    block.Size -= prevLengthAligned;
                    return (T*)block.Alloc(newLengthAligned, false);
                }
            }

            var newArray = AllocArrayUnsafe<T>(newLength);
            UnsafeUtility.MemCpy(newArray, array, prevLengthAligned);

            return newArray;
        }

        /// <summary>
        /// Resets the allocator by setting the used memory size to zero.
        /// </summary>
        public void Clear()
        {
            lastBlock = -1;

            for (int i = 0; i < blocks.Count; ++i)
            {
                blocks.Array[i].Size = 0;
            }
        }

        /// <summary>
        /// Reserves a block of memory of the specified size.
        /// If needed, allocates additional buffer to hold the data.
        /// Optionally, zeroes out the newly allocated memory.
        /// </summary>
        /// <param name="bytes">Number of bytes to allocate</param>
        /// <param name="zero">Indicates whether to zero the memory</param>
        /// <returns>Pointer to the reserved memory</returns>
        private void* Reserve(int bytes, bool zero)
        {
            ImAssert.IsTrue(bytes > 0, "bytes > 0");

            bytes = Align(bytes);

            lastBlock = FindBlockToFit(bytes);
            ref var block = ref blocks.Array[lastBlock];

            return block.Alloc(bytes, zero);
        }

        /// <summary>
        /// Searches for the block with specified size available.
        /// </summary>
        /// <param name="size">Number of bytes to allocate</param>
        /// <returns>Index of found block</returns>
        private int FindBlockToFit(int size)
        {
            for (int i = 0; i < blocks.Count; ++i)
            {
                var free = blocks.Array[i].Capacity - blocks.Array[i].Size;
                if (free < size)
                {
                    continue;
                }

                return i;
            }

            var requiredCapacity = blocks.Array[blocks.Count - 1].Capacity * 2;
            while (requiredCapacity < size)
            {
                requiredCapacity *= 2;
            }

            blocks.Add(new MemoryBlock(requiredCapacity));

            return blocks.Count - 1;
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

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            for (int i = 0; i < blocks.Count; ++i)
            {
                blocks.Array[i].Dispose();
            }

            blocks = default;
            disposed = true;
        }

        internal unsafe struct MemoryBlock: IDisposable
        {
            public readonly int Capacity;
            public readonly IntPtr Ptr;

            public int Size;

            public MemoryBlock(int capacity)
            {
                Size = 0;
                Capacity = capacity;
                Ptr = Marshal.AllocHGlobal(capacity);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void* Alloc(int sizeAligned, bool zero)
            {
                ImAssert.IsTrue(sizeAligned > 0, "sizeAligned > 0");

                var ptr = (void*)(Ptr + Size);
                if (zero)
                {
                    UnsafeUtility.MemSet(ptr, 0, sizeAligned);
                }

                Size += sizeAligned;

                return ptr;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dealloc(void* ptr, int size)
            {
                ImAssert.IsTrue(ptr >= (byte*)Ptr, "ptr >= Ptr");
                ImAssert.IsTrue(ptr < (byte*)(Ptr + Size), "ptr < (Ptr + Size)");

                var remaining = ((byte*)Ptr + Size) - ((byte*)ptr + size);

                ImAssert.IsTrue(remaining >= 0, "remaining >= 0");

                UnsafeUtility.MemMove(ptr, (byte*)ptr + size, remaining);
                Size -= size;
            }

            public void Dispose()
            {
                Marshal.FreeHGlobal(Ptr);
            }
        }
    }
}