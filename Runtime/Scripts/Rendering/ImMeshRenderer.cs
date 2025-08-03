using System;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Rendering
{
    public class ImMeshRenderer: IDisposable
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

        private readonly MaterialPropertyBlock properties;
        
        private Mesh mesh;
        private bool disposed;

        private Material wireframeMaterial;
        private Mesh wireframeMesh;
        private int[] wireframeIndicesBuffer;

        public ImMeshRenderer()
        {
            properties = new MaterialPropertyBlock();
            
            mesh = new Mesh();
            mesh.MarkDynamic();
        }

        public void Render(CommandBuffer cmd, ImMeshBuffer buffer, Vector2 screenSize, float screenScale, Vector2Int targetSize)
        {
            ImProfiler.BeginSample("ImMeshRenderer.Render");

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

                var enableSdfKeyword = new LocalKeyword(meshData.Material.shader, "IMUI_SDF_ON");
                cmd.SetKeyword(meshData.Material, enableSdfKeyword, meshData.GlyphRenderMode == ImGlyphRenderMode.SDFAA);

                if (meshData.MaskRect.Enabled)
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

                if (meshData.ClipRect.Enabled)
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

            ImProfiler.EndSample();
        }

        #region Wireframe Renderer
        
        public void RenderWireframe(CommandBuffer cmd, ImMeshBuffer buffer, Vector2 screenSize, float screenScale)
        {
            if (!wireframeMaterial)
            {
                wireframeMaterial = new Material(Resources.Load<Shader>("Imui/imui_wireframe"));
            }

            if (!wireframeMesh)
            {
                wireframeMesh = new Mesh();
                wireframeMesh.MarkDynamic();
            }
            
            ImProfiler.BeginSample("ImMeshRenderer.RenderWireframe");
        
            wireframeMesh.Clear(true);
            
            var nextSize = Mathf.NextPowerOfTwo(buffer.IndicesCount * 2);
            wireframeIndicesBuffer ??= new int[nextSize];

            if (wireframeIndicesBuffer.Length < nextSize)
            {
                Array.Resize(ref wireframeIndicesBuffer, nextSize);
            }
            
            for (int i = 0; i < buffer.IndicesCount / 3; ++i)
            {
                var a = buffer.Indices[(3 * i) + 0];
                var b = buffer.Indices[(3 * i) + 1];
                var c = buffer.Indices[(3 * i) + 2];
        
                wireframeIndicesBuffer[(i * 6) + 0] = a;
                wireframeIndicesBuffer[(i * 6) + 1] = b;
                wireframeIndicesBuffer[(i * 6) + 2] = b;
                wireframeIndicesBuffer[(i * 6) + 3] = c;
                wireframeIndicesBuffer[(i * 6) + 4] = c;
                wireframeIndicesBuffer[(i * 6) + 5] = a;
            }
        
            wireframeMesh.SetIndexBufferParams(buffer.IndicesCount * 2, IndexFormat.UInt32);
            wireframeMesh.SetVertexBufferParams(buffer.VerticesCount, ImVertex.VertexAttributes);
        
            wireframeMesh.SetVertexBufferData(buffer.Vertices, 0, 0, buffer.VerticesCount, 0, MESH_UPDATE_FLAGS);
            wireframeMesh.SetIndexBufferData(wireframeIndicesBuffer, 0, 0, buffer.IndicesCount * 2, MESH_UPDATE_FLAGS);
        
            if (wireframeMesh.subMeshCount != 1)
            {
                wireframeMesh.subMeshCount = 1;
            }

            var desc = new SubMeshDescriptor()
            {
                topology = MeshTopology.Lines,
                indexStart = 0,
                indexCount = buffer.IndicesCount * 2,
                baseVertex = 0,
                firstVertex = 0,
                vertexCount = buffer.VerticesCount
            };
        
            wireframeMesh.SetSubMesh(0, desc, MESH_UPDATE_FLAGS);
            wireframeMesh.UploadMeshData(false);
            
            screenSize /= screenScale;
        
            var view = Matrix4x4.identity;
            var proj = Matrix4x4.Ortho(0, screenSize.x, 0, screenSize.y, short.MinValue, short.MaxValue);
            var gpuProj = GL.GetGPUProjectionMatrix(proj, true);
        
            cmd.SetGlobalMatrix(ViewProjectionId, view * gpuProj);
            cmd.DrawMesh(wireframeMesh, Matrix4x4.identity, wireframeMaterial, submeshIndex: 0, -1);
    
            ImProfiler.EndSample();
        }
        
        #endregion

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            ImUnityUtility.Destroy(mesh);
            
            if (wireframeMesh)
            {
                ImUnityUtility.Destroy(wireframeMesh);
            }

            if (wireframeMaterial)
            {
                ImUnityUtility.Destroy(wireframeMaterial);
            }
            
            mesh = null;
            wireframeMesh = null;
            wireframeMaterial = null;
            wireframeIndicesBuffer = null;

            disposed = true;
        }
    }
}