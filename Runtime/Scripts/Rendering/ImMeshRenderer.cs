using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Rendering
{
    public class ImMeshRenderer : IDisposable
    {
        private const MeshUpdateFlags MESH_UPDATE_FLAGS =
            MeshUpdateFlags.DontNotifyMeshUsers |
            MeshUpdateFlags.DontRecalculateBounds |
            MeshUpdateFlags.DontResetBoneBounds |
            MeshUpdateFlags.DontValidateIndices;

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int FontTexId = Shader.PropertyToID("_FontTex");
        private static readonly int ViewProjectionId = Shader.PropertyToID("_VP");
        private static readonly int MaskEnabledId = Shader.PropertyToID("_MaskEnable");
        private static readonly int MaskRectId = Shader.PropertyToID("_MaskRect");
        private static readonly int MaskCornerRadiusId = Shader.PropertyToID("_MaskCornerRadius");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        
        #if IMUI_DEBUG
        public bool Wireframe;
        #endif
        
        private readonly MaterialPropertyBlock properties;
        
        private Mesh mesh;
        private bool disposed;
        
        public ImMeshRenderer()
        {
            properties = new MaterialPropertyBlock();
            
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Render(CommandBuffer cmd, ImMeshBuffer buffer, Vector2 size, float scale)
        {
            mesh.Clear(true);
            
            buffer.Trim();
            
            mesh.SetIndexBufferParams(buffer.IndicesCount, IndexFormat.UInt32);
            mesh.SetVertexBufferParams(buffer.VerticesCount, ImVertex.VertexAttributes);

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
                    #if IMUI_DEBUG
                    topology = Wireframe ? MeshTopology.Lines : info.Topology,
                    #else
                    topology = info.Topology,
                    #endif
                    indexStart = info.IndicesOffset,
                    indexCount = info.IndicesCount,
                    baseVertex = 0,
                    firstVertex = info.VerticesOffset,
                    vertexCount = info.VerticesCount
                };

                mesh.SetSubMesh(i, desc, MESH_UPDATE_FLAGS);
            }

            mesh.UploadMeshData(false);

            size /= scale;
            
            var view = Matrix4x4.identity;
            var proj = Matrix4x4.Ortho(0, size.x, 0, size.y, short.MinValue, short.MaxValue);
            var gpuProj = GL.GetGPUProjectionMatrix(proj, true);
            
            cmd.SetGlobalMatrix(ViewProjectionId, view * gpuProj);

            for (int i = 0; i < buffer.MeshesCount; ++i)
            {
                ref var meshData = ref buffer.Meshes[i];

                properties.SetTexture(MainTexId, meshData.MainTex);
                properties.SetTexture(FontTexId, meshData.FontTex);
                properties.SetFloat(ContrastId, meshData.Contrast);
                
                if (meshData.MaskRect.Enabled)
                {
                    var radius = meshData.MaskRect.Radius * scale;
                    var rect = meshData.MaskRect.Rect;
                    var hw = rect.width / 2f;
                    var hh = rect.height / 2f;
                    var vec = new Vector4((rect.x + hw) * scale, (rect.y + hh) * scale, hw * scale, hh * scale);

                    properties.SetInteger(MaskEnabledId, 1);
                    properties.SetVector(MaskRectId, vec);
                    properties.SetFloat(MaskCornerRadiusId, radius);
                }
                else
                {
                    properties.SetInteger(MaskEnabledId, 0);
                }
                
                if (meshData.ClipRect.Enabled)
                {
                    var x = meshData.ClipRect.Rect.xMin * scale;
                    var y = meshData.ClipRect.Rect.yMin * scale;
                    var w = meshData.ClipRect.Rect.width * scale;
                    var h = meshData.ClipRect.Rect.height * scale;
                    var clip = new Rect(x, y, w, h);
                    
                    cmd.EnableScissorRect(clip);
                }
                
                cmd.DrawMesh(mesh, Matrix4x4.identity, meshData.Material, submeshIndex: i, -1, properties);
                
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