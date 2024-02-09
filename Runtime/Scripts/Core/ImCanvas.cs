using System;
using Imui.Rendering;
using Imui.Rendering.Atlas;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Core
{
    public class ImCanvas : IDisposable
    {
        private struct TextureInfo
        {
            public int Id;
            public Texture2D Texture;
            public Vector4 ScaleOffset;
        }
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;

        private static readonly Texture2D DefaultTexture;

        static ImCanvas()
        {
            var pixels = new Color32[DEFAULT_TEX_W * DEFAULT_TEX_H];
            Array.Fill(pixels, Color.white);
            
            DefaultTexture = new Texture2D(DEFAULT_TEX_W, DEFAULT_TEX_H, TextureFormat.RGBA32, false);
            DefaultTexture.SetPixels32(pixels);
            DefaultTexture.Apply();
        }
        
        private const int MAIN_ATLAS_W = 1024;
        private const int MAIN_ATLAS_H = 1024;
        private const float MAIN_ATLAS_IDX = 0;
        
        private const string SHADER_NAME = "imui_default";
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        
        private Shader shader;
        private Material material;
        private TextureAtlas atlas;
        private DynamicArray<TextureInfo> texturesInfo;
        private bool disposed;
        
        private readonly Vector4 defaultTexScaleOffset;
        private readonly MeshDrawer drawer;
        
        public ImCanvas(MeshDrawer drawer)
        {
            this.drawer = drawer;

            shader = Resources.Load<Shader>(SHADER_NAME);
            material = new Material(shader);
            atlas = new TextureAtlas(MAIN_ATLAS_W, MAIN_ATLAS_H);
            texturesInfo = new DynamicArray<TextureInfo>(capacity: 64);

            defaultTexScaleOffset = AddToAtlas(DefaultTexture);
            defaultTexScaleOffset.x *= 0.5f;
            defaultTexScaleOffset.y *= 0.5f;
            defaultTexScaleOffset.z += defaultTexScaleOffset.x / 2.0f;
            defaultTexScaleOffset.w += defaultTexScaleOffset.y / 2.0f;
        }

        public Vector4 AddToAtlas(Texture2D tex)
        {
            var id = tex.GetInstanceID();
            
            for (int i = 0; i < texturesInfo.Count; ++i)
            {
                ref var info = ref texturesInfo.Array[i];
                if (info.Id == id)
                {
                    return info.ScaleOffset;
                }
            }
            
            var scaleOffset = new Vector4();
            atlas.Allocate(tex, ref scaleOffset);
            texturesInfo.Add(new TextureInfo()
            {
                Id = tex.GetInstanceID(),
                ScaleOffset = scaleOffset,
                Texture = tex
            });
            
            return scaleOffset;
        }

        public void Clear()
        {
            drawer.Clear();
            drawer.NextMesh();
            drawer.SetMaterial(material);
            
            material.SetTexture(MainTexId, atlas.AtlasTexture);
        }

        public void Setup(CommandBuffer cmd)
        {
            for (int i = 0; i < texturesInfo.Count; ++i)
            {
                ref var tex = ref texturesInfo.Array[i];
                atlas.Blit(cmd, tex.Texture, ref tex.ScaleOffset);
            }
        }

        public void Rect(ImRect rect, Color32 color)
        {
            drawer.Color = color;
            drawer.ScaleOffset = defaultTexScaleOffset;
            drawer.UVZ = MAIN_ATLAS_IDX;
            drawer.AddQuad(rect.X, rect.Y, rect.W, rect.H, 0);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            Resources.UnloadAsset(shader);
            UnityEngine.Object.Destroy(material);
            atlas.Release();
            
            disposed = true;
        }
    }
}