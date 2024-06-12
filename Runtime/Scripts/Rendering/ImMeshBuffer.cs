using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Imui.Rendering
{
    public class ImMeshBuffer
    {
        public int VerticesCount;
        public int IndicesCount;
        public ImVertex[] Vertices;
        public int[] Indices;
        public ImMeshData[] Meshes;
        public int MeshesCount;

        public ImMeshBuffer(int meshesCapacity, int verticesCapacity, int indicesCapacity)
        {
            Vertices = new ImVertex[verticesCapacity];
            Indices = new int[indicesCapacity];
            Meshes = new ImMeshData[meshesCapacity];
            
            Clear();
        }

        public void Trim()
        {
            if (MeshesCount == 0)
            {
                return;
            }

            ref var meshData = ref Meshes[MeshesCount - 1];
            while ((meshData.VerticesCount == 0 || meshData.IndicesCount == 0) && MeshesCount > 1)
            {
                meshData = ref Meshes[--MeshesCount - 1];
            }
        }
        
        public void Sort()
        {
            var meshes = new Span<ImMeshData>(Meshes, 0, MeshesCount);
            var i = 1;
            while (i < meshes.Length)
            {
                var m = meshes[i];
                var j = i - 1;
                while (j >= 0 && meshes[j].Order > m.Order)
                {
                    meshes[j + 1] = meshes[j];
                    j -= 1;
                }

                meshes[j + 1] = m;
                ++i;
            }
        }
        
        public void Clear()
        {
            MeshesCount = 0;
            VerticesCount = 0;
            IndicesCount = 0;
        }
        
        public void NextMesh()
        {
            if (MeshesCount > 0)
            {
                ref var currentMesh = ref Meshes[MeshesCount - 1];
                if (currentMesh.VerticesCount == 0 && currentMesh.IndicesCount == 0)
                {
                    currentMesh.ClearOptions();
                    return;
                }
            }
            
            EnsureMeshesCapacity(MeshesCount + 1);

            ref var mesh = ref Meshes[MeshesCount++];
            mesh.Clear();
            mesh.IndicesOffset = IndicesCount;
            mesh.VerticesOffset = VerticesCount; 
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureMeshesCapacity(int size)
        {
            if (Meshes.Length < size)
            {
                Array.Resize(ref Meshes, Mathf.NextPowerOfTwo(size));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureVerticesCapacity(int size)
        {
            if (Vertices.Length < size)
            {
                Array.Resize(ref Vertices, Mathf.NextPowerOfTwo(size));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureIndicesCapacity(int size)
        {
            if (Indices.Length < size)
            {
                Array.Resize(ref Indices, Mathf.NextPowerOfTwo(size));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddIndices(int count)
        {
#if IMUI_VALIDATION
            ImuiAssert.False(MeshesCount == 0, "Empty meshes array");
            
            var from = Meshes[MeshesCount - 1].IndicesOffset + Meshes[MeshesCount - 1].IndicesCount;
            var to = from + count;
            
            ImuiAssert.False(to > Indices.Length, "Indices array is too small");

            for (int i = from; i < to; ++i)
            {
                ImuiAssert.True(Indices[i] >= 0 && Indices[i] < Vertices.Length, $"Invalid index {Indices[i]} at {i}");
            }
#endif
            
            IndicesCount += count;
            Meshes[MeshesCount - 1].IndicesCount += count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVertices(int count)
        {
#if IMUI_VALIDATION
            ImuiAssert.False(MeshesCount == 0, "Empty meshes array");
            
            var from = Meshes[MeshesCount - 1].VerticesOffset + Meshes[MeshesCount - 1].VerticesCount;
            var to = from + count;
            
            ImuiAssert.False(to > Vertices.Length, "Vertices array is too small");

            for (int i = from; i < to; ++i)
            {
                var v = Vertices[i];
                if (float.IsNaN(v.Position.x) || float.IsInfinity(v.Position.x) ||
                    float.IsNaN(v.Position.y) || float.IsInfinity(v.Position.y) ||
                    float.IsNaN(v.Position.z) || float.IsInfinity(v.Position.z))
                {
                    ImuiAssert.True(false, "Invalid vertex position, some of the components is either NaN of Infinity");
                }
            }
#endif
            
            VerticesCount += count;
            Meshes[MeshesCount - 1].VerticesCount += count;
        }
    }
}