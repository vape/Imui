using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    public sealed unsafe class ImStorage: IDisposable
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Align(int size) => (sizeof(IntPtr) * ((size + sizeof(IntPtr) - 1) / sizeof(IntPtr)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MakeKey<T>(uint id)
        {
            unchecked
            {
                return (uint)HashCode.Combine(id, TypeHelper<T>.Hash);
            }
        }

        private static class TypeHelper<T>
        {
            public static readonly int Hash = typeof(T).GetHashCode();
        }

        [Flags]
        internal enum MetaFlag: byte
        {
            None = 0,
            Unused = 1
        }

        internal struct Metadata
        {
            public readonly int Type;
            public readonly int Size;
            public readonly byte Block;

            public int Offset;
            public MetaFlag Flags;

            public Metadata(int type, int offset, int size, byte block)
            {
                Type = type;
                Offset = offset;
                Size = size;
                Flags = MetaFlag.None;
                Block = block;
            }
        }

        internal struct UnpairedSegment
        {
            public int Offset;
            public int Size;
            public int Block;

            public UnpairedSegment(int offset, int size, int block)
            {
                Offset = offset;
                Size = size;
                Block = block;
            }
        }

        public int TotalUsed => entriesCount * sizeof(Metadata) + entriesCount * sizeof(uint) + GetTotalOccupiedSize();
        public int TotalAllocated => entriesCapacity * sizeof(Metadata) + entriesCapacity * sizeof(uint) + GetTotalCapacity();

        internal uint* keys;
        internal Metadata* meta;
        internal int entriesCount;
        internal int entriesCapacity;

        private ImDynamicArray<ImArena.MemoryBlock> blocks;
        private ImDynamicArray<UnpairedSegment> unpairedSegments;
        private bool disposed;

        public ImStorage(int entriesCapacity, int memoryCapacity)
        {
            this.entriesCapacity = entriesCapacity;
            entriesCount = 0;
            keys = (uint*)Marshal.AllocHGlobal(this.entriesCapacity * sizeof(uint));
            meta = (Metadata*)Marshal.AllocHGlobal(this.entriesCapacity * sizeof(Metadata));

            blocks = new ImDynamicArray<ImArena.MemoryBlock>(4);
            blocks.Add(new ImArena.MemoryBlock(memoryCapacity));

            unpairedSegments = new ImDynamicArray<UnpairedSegment>(32);
        }

        public int GetTotalOccupiedSize()
        {
            var sum = 0;
            for (int i = 0; i < blocks.Count; ++i)
            {
                sum += blocks.Array[i].Size;
            }

            return sum;
        }

        public int GetTotalCapacity()
        {
            var sum = 0;
            for (int i = 0; i < blocks.Count; ++i)
            {
                sum += blocks.Array[i].Capacity;
            }

            return sum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetArray<T>(uint id, int count) where T: unmanaged => new(GetArrayUnsafe<T>(id, count), count);

        public T* GetArrayUnsafe<T>(uint id, int count) where T: unmanaged
        {
            ImProfiler.BeginSample("ImStorage.GetArrayUnsafe<T>");

            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index))
            {
                var result = InsertArray<T>(index, key, count);

                ImProfiler.EndSample();
                return result;
            }

            if (meta[index].Type != TypeHelper<T>.Hash)
            {
                Delete(index);

                var result = InsertArray<T>(index, key, count);

                ImProfiler.EndSample();
                return result;
            }

            if (meta[index].Size != Align(sizeof(T) * count))
            {
                var result = ResizeArray<T>(index, key, count);

                ImProfiler.EndSample();
                return result;
            }

            var m = &meta[index];
            m->Flags &= ~MetaFlag.Unused;
            var ptr = (T*)(blocks.Array[m->Block].Ptr + meta[index].Offset);

            ImProfiler.EndSample();
            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint id, T def = default) where T: unmanaged => ref *GetUnsafe(id, def);

        public T* GetUnsafe<T>(uint id, T def = default) where T: unmanaged
        {
            ImProfiler.BeginSample("ImStorage.GetUnsafe<T>");

            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index))
            {
                var result = Insert(index, key, def);

                ImProfiler.EndSample();
                return result;
            }

            if (meta[index].Type != TypeHelper<T>.Hash)
            {
                Delete(index);
                var result = Insert(index, key, def);

                ImProfiler.EndSample();
                return result;
            }

            var m = &meta[index];
            m->Flags &= ~MetaFlag.Unused;
            var ptr = (T*)(blocks.Array[m->Block].Ptr + m->Offset);

            ImProfiler.EndSample();

            return ptr;
        }

        public bool Remove<T>(uint id)
        {
            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index) || meta[index].Type != TypeHelper<T>.Hash)
            {
                return false;
            }

            Delete(index);
            return true;
        }

        public void FindUnused()
        {
            ImProfiler.BeginSample("ImStorage.FindUnused");

            var limit = unpairedSegments.Array.Length - unpairedSegments.Count;

            for (int i = 0; i < entriesCount && limit > 0; ++i)
            {
                if ((meta[i].Flags & MetaFlag.Unused) != 0)
                {
                    limit--;
                    Delete(i);
                    continue;
                }

                meta[i].Flags |= MetaFlag.Unused;
            }

            ImProfiler.EndSample();
        }

        public bool Collect()
        {
            ImProfiler.BeginSample("ImStorage.Collect");

            ImProfiler.BeginSample("ImStorage.Collect/SortUnpairedSegments");

            var k = 1;
            while (k < unpairedSegments.Count)
            {
                var s = unpairedSegments.Array[k];
                var j = k - 1;

                while (j >= 0 && unpairedSegments.Array[j].Offset > s.Offset)
                {
                    unpairedSegments.Array[j + 1] = unpairedSegments.Array[j];
                    j -= 1;
                }

                unpairedSegments.Array[j + 1] = s;
                ++k;
            }

            ImProfiler.EndSample();

            for (int i = unpairedSegments.Count - 1; i >= 0; --i)
            {
                Release(unpairedSegments.Array[i]);
            }

            unpairedSegments.Clear(false);

            ImProfiler.EndSample();

            return false;
        }

        private void Delete(int index)
        {
            ImAssert.IsTrue(entriesCount > 0, "entriesCount > 0");
            ImAssert.IsTrue(index >= 0, "index >= 0");
            ImAssert.IsTrue(index < entriesCount, "index < entriesCount");

            ImProfiler.BeginSample("ImStorage.Delete");

            var offset = meta[index].Offset;
            var size = meta[index].Size;
            var block = meta[index].Block;

            if (index != entriesCount - 1)
            {
                ShiftEntries(index + 1, -1);
            }

            entriesCount--;

            var merged = false;

            for (int i = 0; i < unpairedSegments.Count; ++i)
            {
                ref var p = ref unpairedSegments.Array[i];
                if (p.Block != block)
                {
                    continue;
                }

                if (p.Offset == (offset + size))
                {
                    p.Offset = offset;
                    p.Size += size;
                    merged = true;
                    break;
                }

                if (offset == (p.Offset + p.Size))
                {
                    p.Size += size;
                    merged = true;
                    break;
                }
            }

            if (!merged)
            {
                unpairedSegments.Add(new UnpairedSegment(offset, size, block));
            }

            ImProfiler.EndSample();
        }

        private void Release(UnpairedSegment segment)
        {
            ImAssert.IsTrue(segment.Block >= 0, "segment.Block >= 0");
            ImAssert.IsTrue(segment.Block < blocks.Count, "segment.Block < blocks.Count");

            ref var block = ref blocks.Array[segment.Block];

            ImAssert.IsTrue(segment.Offset > 0, "segment.Offset > 0");
            ImAssert.IsTrue(segment.Offset < block.Capacity, "segment.Offset < dataSize");
            ImAssert.IsTrue(segment.Size <= block.Size, "segment.Size <= dataSize");

            block.Dealloc((void*)(block.Ptr + segment.Offset), segment.Size);

            for (int i = 0; i < entriesCount; ++i)
            {
                var m = &meta[i];
                if (m->Block == segment.Block && m->Offset > segment.Offset)
                {
                    m->Offset -= segment.Size;
                }
            }
        }

        private T* Insert<T>(int index, uint key, T value) where T: unmanaged
        {
            ImAssert.IsTrue(index >= 0, "index >= 0");
            ImAssert.IsTrue(index <= entriesCount, "index <= count");

            var sizeAligned = Align(sizeof(T));
            var blockIndex = FindBlockToFit(sizeAligned);

            ImAssert.IsTrue(blockIndex <= byte.MaxValue, "blockIndex <= byte.MaxValue");
            ImAssert.IsTrue(blockIndex >= 0, "blockIndex >= 0");
            ImAssert.IsTrue(blockIndex < blocks.Count, "blockIndex < blocks.Count");

            ref var block = ref blocks.Array[blockIndex];

            if (entriesCapacity <= entriesCount)
            {
                GrowEntriesToFit(entriesCapacity + 1);
            }

            if (index < entriesCount)
            {
                ShiftEntries(index, 1);
            }

            keys[index] = key;
            meta[index] = new Metadata(TypeHelper<T>.Hash, block.Size, sizeAligned, (byte)blockIndex);
            entriesCount++;

            var ptr = (T*)block.Alloc(sizeAligned, false);
            *ptr = value;

            return ptr;
        }

        private T* ResizeArray<T>(int index, uint key, int count) where T: unmanaged
        {
            ImAssert.IsTrue(index >= 0, "index >= 0");
            ImAssert.IsTrue(index <= entriesCount, "index <= count");

            var m = &meta[index];
            ref var block = ref blocks.Array[m->Block];

            var oldPtr = (T*)(block.Ptr + m->Offset);
            var oldSize = m->Size;

            Delete(index);
            var ptr = InsertArray<T>(index, key, count);

            UnsafeUtility.MemCpy(ptr, oldPtr, oldSize);

            return ptr;
        }

        private T* InsertArray<T>(int index, uint key, int count, bool zero = true) where T: unmanaged
        {
            ImAssert.IsTrue(index >= 0, "index >= 0");
            ImAssert.IsTrue(index <= entriesCount, "index <= count");

            var sizeAligned = Align(sizeof(T) * count);
            var blockIndex = FindBlockToFit(sizeAligned);

            ImAssert.IsTrue(blockIndex <= byte.MaxValue, "block <= byte.MaxValue");

            ref var block = ref blocks.Array[blockIndex];

            if (entriesCapacity <= entriesCount)
            {
                GrowEntriesToFit(entriesCapacity + 1);
            }

            if (index < entriesCount)
            {
                ShiftEntries(index, 1);
            }

            keys[index] = key;
            meta[index] = new Metadata(TypeHelper<T>.Hash, block.Size, sizeAligned, (byte)blockIndex);
            entriesCount++;

            return (T*)block.Alloc(sizeAligned, zero);
        }

        private void ShiftEntries(int index, int offset)
        {
            var keysDst = keys + index + offset;
            var keysSrc = keys + index;
            var metaDst = meta + index + offset;
            var metaSrc = meta + index;

            UnsafeUtility.MemMove(keysDst, keysSrc, sizeof(uint) * (entriesCount - index));
            UnsafeUtility.MemMove(metaDst, metaSrc, sizeof(Metadata) * (entriesCount - index));
        }

        private bool TryFindIndex(uint key, out int index)
        {
            ImProfiler.BeginSample("ImStorage.TryFindIndex");

            var l = 0;
            var h = entriesCount - 1;

            while (l <= h)
            {
                var m = l + (h - l) / 2;

                if (keys[m] == key)
                {
                    index = m;

                    ImProfiler.EndSample();
                    return true;
                }

                if (keys[m] < key)
                {
                    l = m + 1;
                }
                else
                {
                    h = m - 1;
                }
            }

            index = l;

            ImProfiler.EndSample();
            return false;
        }

        private void GrowEntriesToFit(int capacity)
        {
            if (capacity < entriesCapacity)
            {
                return;
            }

            var nextCapacity = entriesCapacity * 2;
            while (nextCapacity < capacity)
            {
                nextCapacity *= 2;
            }

            keys = (uint*)Marshal.ReAllocHGlobal((IntPtr)keys, (IntPtr)(nextCapacity * sizeof(uint)));
            meta = (Metadata*)Marshal.ReAllocHGlobal((IntPtr)meta, (IntPtr)(nextCapacity * sizeof(Metadata)));
            entriesCapacity = nextCapacity;
        }

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

            blocks.Add(new ImArena.MemoryBlock(requiredCapacity));

            return blocks.Count - 1;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // ReSharper disable once UnusedParameter.Local
        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            Marshal.FreeHGlobal((IntPtr)keys);
            Marshal.FreeHGlobal((IntPtr)meta);

            for (int i = 0; i < blocks.Count; ++i)
            {
                blocks.Array[i].Dispose();
            }

            blocks.Clear(false);
            meta = null;
            keys = null;

            entriesCount = 0;
            entriesCapacity = 0;

            disposed = true;
        }

        ~ImStorage()
        {
            Dispose(false);
        }
    }
}