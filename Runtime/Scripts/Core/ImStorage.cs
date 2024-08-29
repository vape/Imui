using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace Imui.Core
{
    public sealed unsafe class ImStorage : IDisposable
    {
        [Flags]
        private enum Flag : byte
        {
            // ReSharper disable once UnusedMember.Local
            None = 0,
            Used = 1 << 0
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Metadata
        {
            public Flag Flag;
            public byte Size;
            public uint Id;
        }

        public int OccupiedSize => (int)tail - (int)data;
        public int Capacity => capacity;
        
        private int capacity;
        private IntPtr data;
        private IntPtr tail;
        private bool disposed;

        public ImStorage(int capacity)
        {
            this.capacity = capacity;
            
            data = Marshal.AllocHGlobal(capacity);
            tail = data;
        }

        public ref T Get<T>(uint id, T defaultValue = default) where T : unmanaged
        {
            return ref *GetRef(id, defaultValue);
        }
        
        public T* GetRef<T>(uint id, T defaultValue = default) where T : unmanaged
        {
            if (!TryGet(out T* value, out Metadata* metadata, id))
            {
                return AddValue(id, defaultValue);
            }

            metadata->Flag |= Flag.Used;
            return value;
        }

        public bool TryGetRef<T>(uint id, out T* value) where T : unmanaged
        {
            var result = TryGet(out value, out var metadata, id);
            if (result)
            {
                metadata->Flag |= Flag.Used;
            }

            return result;
        }

        public void CollectAndCompact()
        {
            var ptr = data;
            var free = 0;

            while ((int)ptr < (int)tail)
            {
                var metadata = (Metadata*)ptr;
                var blockSize = sizeof(Metadata) + metadata->Size;
                
                if ((metadata->Flag & Flag.Used) != 0)
                {
                    metadata->Flag &= ~Flag.Used;

                    if (free > 0)
                    {
                        Buffer.MemoryCopy((void*)ptr, (void*)(ptr - free), blockSize, blockSize);
                    }
                }
                else
                {
                    free += blockSize;
                }
                
                ptr += blockSize;
            }
            
            tail -= free;
        }

        private T* AddValue<T>(uint id, T value = default) where T : unmanaged
        {
            Assert.IsTrue(sizeof(T) <= byte.MaxValue);
            
            var size = (byte)sizeof(T);
            if (((int)tail - (int)data + size + sizeof(Metadata)) >= capacity)
            {
                Grow();
            }
            
            var metadata = new Metadata()
            {
                Flag = Flag.Used, 
                Id = id, 
                Size = size
            };

            var ptr = (void*)tail;
            
            SetMetaAndValue(ptr, ref metadata, ref value);
            tail += size + sizeof(Metadata);
            
            return GetValueRef<T>(ptr);
        }

        private bool TryGet<T>(out T* value, out Metadata* metadata, uint id) where T: unmanaged
        {
            var size = sizeof(T);
            var ptr = data;
            
            while ((int)ptr < (int)tail)
            {
                metadata = (Metadata*)ptr;

                if (metadata->Id != id || metadata->Size != size)
                {
                    ptr += sizeof(Metadata) + metadata->Size;
                    continue;
                }

                value = GetValueRef<T>((void*)ptr);
                return true;
            }

            value = null;
            metadata = null;
            return false;
        }
        
        private void Grow(int ratio = 2)
        {
            if (data == IntPtr.Zero)
            {
                throw new NullReferenceException();
            }
            
            var nextCapacity = capacity * ratio;
            var nextData = Marshal.AllocHGlobal(nextCapacity);
            var sizeToCopy = (int)tail - (int)data;
            
            Buffer.MemoryCopy((void*)data, (void*)nextData, nextCapacity, sizeToCopy);
            Marshal.FreeHGlobal(data);

            capacity = nextCapacity;
            data = nextData;
            tail = data + sizeToCopy;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T* GetValueRef<T>(void* ptr) where T : unmanaged
        {
            return (T*)((byte*)ptr + sizeof(Metadata));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMetaAndValue<T>(void* ptr, ref Metadata metadata, ref T value) where T: unmanaged 
        {
            *(Metadata*)ptr = metadata;
            *(T*)((byte*)ptr + sizeof(Metadata)) = value;
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
            
            Marshal.FreeHGlobal(data);

            data = IntPtr.Zero;
            tail = IntPtr.Zero;

            disposed = true;
        }

        ~ImStorage()
        {
            Dispose(false);
        }
    }
}