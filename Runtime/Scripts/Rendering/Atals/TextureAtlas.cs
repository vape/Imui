#if !IMUI_USING_CORE_RP

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Imui.Rendering.Atlas
{
    internal class TextureAtlas
    {
        private const string BLIT_SHADER_NAME = "imui_atlas_blit";
        
        private static readonly int MainTexShaderProp = Shader.PropertyToID("_MainTex");
        private static readonly int ScaleOffsetShaderProp = Shader.PropertyToID("_ScaleOffset");
        
        private static readonly VertexAttributeDescriptor[] VertexAttributes = new VertexAttributeDescriptor[]
        {
            new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Vertex
        {
            public Vector3 Position;
            public Vector2 UV;

            public Vertex(float x, float y)
            {
                Position = new Vector3(x, y);
                UV = new Vector2(x, y);
            }
        }
        
        private struct TextureData
        {
            public Vector4 ScaleOffset;
            public bool Blitted;
        }
        
        public Texture AtlasTexture => atlasTexture;

        private TextureAtlasAllocator allocator;
        private RenderTexture atlasTexture;
        private Dictionary<int, TextureData> textures;
        private Shader shader;
        private Material material;
        private MaterialPropertyBlock propertyBlock;
        private Mesh quadMesh;

        public TextureAtlas(int width, int height)
        {
            allocator = new TextureAtlasAllocator(width, height, false);
            atlasTexture = new RenderTexture(1024, 1024, 0, RenderTextureFormat.ARGB32);
            atlasTexture.name = "ImuiAtlas";
            textures = new Dictionary<int, TextureData>();
            shader = Resources.Load<Shader>(BLIT_SHADER_NAME);
            material = new Material(shader);
            propertyBlock = new MaterialPropertyBlock();

            quadMesh = new Mesh();
            SetupMesh(quadMesh, new []
            {
                new Vertex(0, 0), 
                new Vertex(0, 1), 
                new Vertex(1, 1), 
                new Vertex(1, 0)
            }, new []{ 0, 1, 2, 2, 3, 0 });
        }
        
        public void Allocate(Texture tex, ref Vector4 scaleOffset)
        {
            var id = tex.GetInstanceID();
            if (textures.TryGetValue(id, out var data))
            {
                scaleOffset = data.ScaleOffset;
                return;
            }
            
            scaleOffset = new Vector4();
            
            if (!allocator.Allocate(ref scaleOffset, tex.width, tex.height))
            {
                throw new Exception($"Failed to allocate atlas space for texture {tex.name}");
            }

            scaleOffset = new Vector4(
                scaleOffset.x / atlasTexture.width, 
                scaleOffset.y / atlasTexture.height,
                scaleOffset.z / atlasTexture.width, 
                scaleOffset.w / atlasTexture.height);
            // scaleOffset.Scale(new Vector4(1.0f / atlasTexture.width, 1.0f / atlasTexture.height, 1.0f / atlasTexture.width, 1.0f / atlasTexture.height));
            textures.Add(id, new TextureData()
            {
                ScaleOffset = scaleOffset,
                Blitted = false
            });
        }

        public void Blit(CommandBuffer cmd, Texture tex, ref Vector4 scaleOffset)
        {
            var id = tex.GetInstanceID();
            if (!textures.TryGetValue(id, out var data))
            {
                Allocate(tex, ref scaleOffset);
                data = textures[id];
                scaleOffset = data.ScaleOffset;
            }

            if (data.Blitted)
            {
                return;
            }
            
            var so = new Vector4(tex.width, tex.height, scaleOffset.z * atlasTexture.width, scaleOffset.w * atlasTexture.height);
            var matrix = Matrix4x4.Ortho(0, atlasTexture.width, 0, atlasTexture.height, -1, 1);
            
            propertyBlock.SetTexture(MainTexShaderProp, tex);
            propertyBlock.SetVector(ScaleOffsetShaderProp, so);
            
            cmd.SetRenderTarget(atlasTexture);
            cmd.DrawMesh(quadMesh, matrix, material, 0, 0, propertyBlock);
            
            data.Blitted = true;
            textures[id] = data;
        }

        public void Release()
        {
            Resources.UnloadAsset(shader);
            
            Object.Destroy(quadMesh);
            Object.Destroy(material);
            
            atlasTexture.Release();
        }
        
        private static void SetupMesh(Mesh Mesh, Vertex[] vertices, int[] indices)
        {
            const MeshUpdateFlags MESH_UPDATE_FLAGS =
                MeshUpdateFlags.DontNotifyMeshUsers |
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontValidateIndices;
            
            Mesh.Clear(true);
            
            Mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt32);
            Mesh.SetVertexBufferParams(vertices.Length, VertexAttributes);
            Mesh.SetVertexBufferData(vertices, 0, 0, vertices.Length, 0, MESH_UPDATE_FLAGS);
            Mesh.SetIndexBufferData(indices, 0, 0, indices.Length, MESH_UPDATE_FLAGS);
            
            Mesh.subMeshCount = 1;
            
            var descriptor = new SubMeshDescriptor(0, indices.Length) { vertexCount = vertices.Length };
            Mesh.SetSubMesh(0, descriptor, MESH_UPDATE_FLAGS);
            
            Mesh.UploadMeshData(false);
        }
    }
}

#else

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Imui.Rendering
{
    internal class TextureAtlas
    {
        private const string TEX_NAME = "ImuiAtlas";
        
        public Texture AtlasTexture => atlas.AtlasTexture;
        
        private Texture2DAtlas atlas;
        
        public TextureAtlas(int width, int height)
        {
            atlas = new Texture2DAtlas(width, height, GraphicsFormat.R8G8B8A8_UNorm, name: TEX_NAME, useMipMap: false);
        }

        public void Allocate(Texture tex, ref Vector4 scaleOffset)
        {
            atlas.AllocateTextureWithoutBlit(tex, tex.width, tex.height, ref scaleOffset);
        }

        public void Blit(CommandBuffer cmd, Texture tex, ref Vector4 scaleOffset)
        {
            atlas.UpdateTexture(cmd, tex, ref scaleOffset, blitMips: false);
        }

        public void Release()
        {
            atlas.Release();
        }
    }
}

#endif