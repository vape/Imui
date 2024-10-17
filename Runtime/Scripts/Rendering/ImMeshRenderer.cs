using System;
using Imui.Utility;
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
        private static readonly int InvColorMul = Shader.PropertyToID("_InvColorMul");
        
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

        public void Render(CommandBuffer cmd, ImMeshBuffer buffer, Vector2 screenSize, float screenScale, Vector2Int targetSize)
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

            var maskScale = screenScale * new Vector2(targetSize.x / screenSize.x, targetSize.y / screenSize.y);
            var maskRadiusScale = Mathf.Min(maskScale.x, maskScale.y);
            
            screenSize /= screenScale;
            
            var view = Matrix4x4.identity;
            var proj = Matrix4x4.Ortho(0, screenSize.x, 0, screenSize.y, short.MinValue, short.MaxValue);
            var gpuProj = GL.GetGPUProjectionMatrix(proj, true);
            
            cmd.SetGlobalMatrix(ViewProjectionId, view * gpuProj);

            for (int i = 0; i < buffer.MeshesCount; ++i)
            {
                ref var meshData = ref buffer.Meshes[i];

                properties.SetTexture(MainTexId, meshData.MainTex);
                properties.SetTexture(FontTexId, meshData.FontTex);
                properties.SetFloat(InvColorMul, meshData.InvColorMul);
                
                if (false) // meshData.MaskRect.Enabled)
                {
                    var radius = meshData.MaskRect.Radius * maskRadiusScale;
                    var rect = meshData.MaskRect.Rect;
                    var hw = rect.width / 2f;
                    var hh = rect.height / 2f;
                    var vec = new Vector4((rect.x + hw) * maskScale.x, (rect.y + hh) * maskScale.y, hw * maskScale.x, hh * maskScale.y);

                    properties.SetInteger(MaskEnabledId, 1);
                    properties.SetVector(MaskRectId, vec);
                    properties.SetFloat(MaskCornerRadiusId, radius);
                }
                else
                {
                    properties.SetInteger(MaskEnabledId, 0);
                }
                
                if (false) // meshData.ClipRect.Enabled)
                {
                    var x = meshData.ClipRect.Rect.xMin * maskScale.x;
                    var y = meshData.ClipRect.Rect.yMin * maskScale.y;
                    var w = meshData.ClipRect.Rect.width * maskScale.x;
                    var h = meshData.ClipRect.Rect.height * maskScale.y;
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
            
            ImObjectUtility.Destroy(mesh);
            mesh = null;

            disposed = true;
        }
    }
}