using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Rendering
{
    public class MeshRenderer : IDisposable
    {
        private const MeshUpdateFlags MESH_UPDATE_FLAGS =
            MeshUpdateFlags.DontNotifyMeshUsers |
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontResetBoneBounds |
            MeshUpdateFlags.DontValidateIndices;

        private static readonly int ViewProjectionId = Shader.PropertyToID("_VP");
        
        private Mesh mesh;
        private bool disposed;
        
        public MeshRenderer()
        {
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Render(CommandBuffer cmd, Rect screen, MeshBuffer buffer)
        {
            mesh.Clear(true);
            
            buffer.Trim();
            
            mesh.SetIndexBufferParams(buffer.IndicesCount, IndexFormat.UInt32);
            mesh.SetVertexBufferParams(buffer.VerticesCount, Vertex.VertexAttributes);

            mesh.SetVertexBufferData(buffer.Vertices, 0, 0, buffer.VerticesCount, 0, MESH_UPDATE_FLAGS);
            mesh.SetIndexBufferData(buffer.Indices, 0, 0, buffer.IndicesCount, MESH_UPDATE_FLAGS);
            
            if (mesh.subMeshCount != buffer.MeshesCount)
            {
                mesh.subMeshCount = buffer.MeshesCount;
            }

            buffer.Sort();

            for (int i = 0; i < buffer.MeshesCount; ++i)
            {
                ref var info = ref buffer.Meshes[i];

                var desc = new SubMeshDescriptor()
                {
                    topology = info.Topology,
                    indexStart = info.IndicesOffset,
                    indexCount = info.IndicesCount,
                    baseVertex = 0,
                    firstVertex = info.VerticesOffset,
                    vertexCount = info.VerticesCount
                };

                mesh.SetSubMesh(i, desc, MESH_UPDATE_FLAGS);
            }

            mesh.UploadMeshData(false);

            var view = Matrix4x4.identity;
            var proj = Matrix4x4.Ortho(screen.xMin, screen.xMax, screen.yMin, screen.yMax, short.MinValue, short.MaxValue);
            var gpuProj = GL.GetGPUProjectionMatrix(proj, true);
            
            cmd.SetGlobalMatrix(ViewProjectionId, view * gpuProj);

            for (int i = 0; i < buffer.MeshesCount; ++i)
            {
                ref var meshData = ref buffer.Meshes[i];
                
                if (meshData.ClipRect.Enabled)
                {
                    cmd.EnableScissorRect(meshData.ClipRect.Rect);
                }
                
                cmd.DrawMesh(mesh, Matrix4x4.identity, meshData.Material, submeshIndex: i, -1);
                
                if (meshData.ClipRect.Enabled)
                {
                    cmd.DisableScissorRect();
                }
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            UnityEngine.Object.Destroy(mesh);
            mesh = null;

            disposed = true;
        }
    }
}