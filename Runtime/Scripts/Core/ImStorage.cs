using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Imui.Utility;
using Unity.Collections.LowLevel.Unsafe;

namespace Imui.Core
{
    public sealed unsafe class ImStorage : IDisposable
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
        internal enum MetaFlag : short
        {
            None = 0,
            Unused = 1,
            Pinned = 2
        }

        internal struct Metadata
        {
            public readonly int Type;
            public readonly int Size;
            
            public int Offset;
            public uint Parent;
            public MetaFlag Flags;

            public Metadata(int type, int offset, int size)
            {
                Type = type;
                Offset = offset;
                Size = size;
                Parent = 0;
                Flags = MetaFlag.None;
            }
        }

        internal struct Scope
        { }

        public int TotalUsed => entriesCount * sizeof(Metadata) + entriesCapacity * sizeof(uint) + dataSize;
        public int TotalAllocated => entriesCapacity * sizeof(Metadata) + entriesCapacity * sizeof(uint) + dataCapacity;

        internal uint* keys;
        internal Metadata* meta;
        internal int entriesCount;
        internal int entriesCapacity;

        internal byte* data;
        internal int dataSize;
        internal int dataCapacity;

        private ImDynamicArray<uint> scopesStack;
        private bool disposed;

        public ImStorage(int initialEntriesCapacity)
        {
            entriesCapacity = initialEntriesCapacity;
            entriesCount = 0;
            keys = (uint*)Marshal.AllocHGlobal(entriesCapacity * sizeof(uint));
            meta = (Metadata*)Marshal.AllocHGlobal(entriesCapacity * sizeof(Metadata));

            dataCapacity = initialEntriesCapacity * 64;
            data = (byte*)Marshal.AllocHGlobal(dataCapacity);
            
            scopesStack = new ImDynamicArray<uint>(32);
        }

        public void BeginPinned(uint id)
        {
            GetPtr<Scope>(id);
            
            scopesStack.Push(MakeKey<Scope>(id));
        }

        public void EndPinned()
        {
            scopesStack.Pop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> GetArray<T>(uint id, int count) where T: unmanaged => new Span<T>(GetPtrArray<T>(id, count), count);
        public T* GetPtrArray<T>(uint id, int count) where T: unmanaged
        {
            ImProfiler.BeginSample("ImStorage.GetPtrArray<T>");
            
            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index))
            {
                return InsertArray<T>(index, key, count);
            }
            
            meta[index].Flags &= ~MetaFlag.Unused;
            var ptr = (T*)(data + meta[index].Offset);

            ImProfiler.EndSample();
            
            return ptr;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint id, T def = default) where T: unmanaged => ref *GetPtr(id, def);
        public T* GetPtr<T>(uint id, T def = default) where T: unmanaged
        {
            ImProfiler.BeginSample("ImStorage.GetPtr<T>");

            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index))
            {
                return Insert(index, key, def);
            }

            if (meta[index].Type != TypeHelper<T>.Hash)
            {
                Delete(index);
                
                return Insert(index, key, def);
            }

            meta[index].Flags &= ~MetaFlag.Unused;
            var ptr = (T*)(data + meta[index].Offset);

            ImProfiler.EndSample();

            return ptr;
        }
        
        public bool TryGetPtr<T>(uint id, out T* value) where T: unmanaged
        {
            ImProfiler.BeginSample("ImStorage.TryGetPtr<T>");

            var key = MakeKey<T>(id);
            if (!TryFindIndex(key, out var index) || meta[index].Type != TypeHelper<T>.Hash)
            {
                value = default;
                return false;
            }

            meta[index].Flags &= ~MetaFlag.Unused;
            value = (T*)(data + meta[index].Offset);

            ImProfiler.EndSample();

            return true;
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
        
        public bool CollectAndCompactIteration()
        {
            ImProfiler.BeginSample("ImStorage.CollectAndCompactIteration");

            for (int i = 0; i < entriesCount; ++i)
            {
                if ((meta[i].Flags & MetaFlag.Unused) != 0 && (meta[i].Flags & MetaFlag.Pinned) == 0)
                {
                    Delete(i);
                    return true;
                }

                meta[i].Flags |= MetaFlag.Unused;
            }

            ImProfiler.EndSample();

            return false;
        }

        private void Delete(int index)
        {
            ImAssert.True(entriesCount > 0, "entriesCount > 0");
            ImAssert.True(index >= 0, "index >= 0");
            ImAssert.True(index < entriesCount, "index < entriesCount");

            ImProfiler.BeginSample("ImStorage.Delete");

            var id = keys[index];
            var offset = meta[index].Offset;
            var size = meta[index].Size;

            if (index != entriesCount - 1)
            {
                ShiftEntries(index + 1, -1);
            }

            entriesCount--;

            UnsafeUtility.MemMove(data + offset, data + offset + size, dataSize - size - offset);

            for (int i = 0; i < entriesCount; ++i)
            {
                if (meta[i].Offset > offset)
                {
                    meta[i].Offset -= size;
                }

                if ((meta[i].Flags & MetaFlag.Pinned) != 0 && meta[i].Parent == id)
                {
                    meta[i].Flags &= ~MetaFlag.Pinned;
                }
            }

            dataSize -= size;

            ImProfiler.EndSample();
        }

        private T* Insert<T>(int index, uint key, T value) where T: unmanaged
        {
            ImAssert.True(index >= 0, "index >= 0");
            ImAssert.True(index <= entriesCount, "index <= count");
            ImAssert.True(sizeof(T) <= short.MaxValue, "sizeof(T) <= short.MaxValue");

            var sizeAligned = Align(sizeof(T));

            if (entriesCapacity <= entriesCount)
            {
                GrowEntriesToFit(entriesCapacity + 1);
            }

            if ((dataCapacity - dataSize) < sizeAligned)
            {
                GrowDataToFit(dataCapacity + sizeAligned);
            }

            if (index < entriesCount)
            {
                ShiftEntries(index, 1);
            }

            keys[index] = key;
            meta[index] = new Metadata(TypeHelper<T>.Hash, dataSize, (short)sizeAligned);

            if (scopesStack.TryPeek(out var parent))
            {
                meta[index].Flags |= MetaFlag.Pinned;
                meta[index].Parent = parent;
            }

            var ptr = (T*)(data + dataSize);
            *ptr = value;

            entriesCount++;
            dataSize += sizeAligned;

            return ptr;
        }
        
        private T* InsertArray<T>(int index, uint key, int count) where T: unmanaged
        {
            ImAssert.True(index >= 0, "index >= 0");
            ImAssert.True(index <= entriesCount, "index <= count");

            var sizeAligned = Align(sizeof(T) * count);

            if (entriesCapacity <= entriesCount)
            {
                GrowEntriesToFit(entriesCapacity + 1);
            }

            if ((dataCapacity - dataSize) < sizeAligned)
            {
                GrowDataToFit(dataCapacity + sizeAligned);
            }

            if (index < entriesCount)
            {
                ShiftEntries(index, 1);
            }

            keys[index] = key;
            meta[index] = new Metadata(TypeHelper<T>.Hash, dataSize, sizeAligned);
            
            if (scopesStack.TryPeek(out var parent))
            {
                meta[index].Flags |= MetaFlag.Pinned;
                meta[index].Parent = parent;
            }

            var ptr = (T*)(data + dataSize);
            UnsafeUtility.MemSet(ptr, 0, sizeAligned);

            entriesCount++;
            dataSize += sizeAligned;

            return ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            var l = 0;
            var h = entriesCount - 1;

            while (l <= h)
            {
                var m = l + (h - l) / 2;

                if (keys[m] == key)
                {
                    index = m;
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

        private void GrowDataToFit(int capacity)
        {
            if (capacity <= dataCapacity)
            {
                return;
            }

            var nextCapacity = dataCapacity * 2;
            while (nextCapacity < capacity)
            {
                nextCapacity *= 2;
            }

            data = (byte*)Marshal.ReAllocHGlobal((IntPtr)data, (IntPtr)nextCapacity);
            dataCapacity = nextCapacity;
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
            Marshal.FreeHGlobal((IntPtr)data);

            data = null;
            keys = null;
            data = null;

            entriesCount = 0;
            entriesCapacity = 0;
            dataSize = 0;
            dataCapacity = 0;

            disposed = true;
        }

        ~ImStorage()
        {
            Dispose(false);
        }
    }
}