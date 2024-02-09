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

        public struct MeshProperties
        {
            public Material Material;
            public MeshClipRect ClipRect;
            public int Order;
        }
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;
                
        private const int MAIN_ATLAS_W = 1024;
        private const int MAIN_ATLAS_H = 1024;
        private const float MAIN_ATLAS_IDX = 0;

        private const int MESH_PROP_CAPACITY = 16;
        
        private const string SHADER_NAME = "imui_default";
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        
        private static readonly Texture2D DefaultTexture;

        static ImCanvas()
        {
            var pixels = new Color32[DEFAULT_TEX_W * DEFAULT_TEX_H];
            Array.Fill(pixels, Color.white);
            
            DefaultTexture = new Texture2D(DEFAULT_TEX_W, DEFAULT_TEX_H, TextureFormat.RGBA32, false);
            DefaultTexture.SetPixels32(pixels);
            DefaultTexture.Apply();
        }

        private Shader shader;
        private Material material;
        private TextureAtlas atlas;
        private DynamicArray<TextureInfo> texturesInfo;
        private DynamicArray<MeshProperties> meshPropertiesStack;
        private Rect screen;
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
            meshPropertiesStack = new DynamicArray<MeshProperties>(MESH_PROP_CAPACITY);

            defaultTexScaleOffset = AddToAtlas(DefaultTexture);
            defaultTexScaleOffset.x *= 0.5f;
            defaultTexScaleOffset.y *= 0.5f;
            defaultTexScaleOffset.z += defaultTexScaleOffset.x / 2.0f;
            defaultTexScaleOffset.w += defaultTexScaleOffset.y / 2.0f;
        }

        public void Begin(Rect screen)
        {
            this.screen = screen;
            
            material.SetTexture(MainTexId, atlas.AtlasTexture);
            
            drawer.Clear();

            var defaultProperties = GetDefaultMeshProperties();
            PushMeshProperties(ref defaultProperties);
        }

        public void End()
        {
            meshPropertiesStack.Pop();
            
            ImuiAssert.True(meshPropertiesStack.Count == 0, "Mesh properties stack is not empty!");
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
        
        public void Setup(CommandBuffer cmd)
        {
            for (int i = 0; i < texturesInfo.Count; ++i)
            {
                ref var tex = ref texturesInfo.Array[i];
                atlas.Blit(cmd, tex.Texture, ref tex.ScaleOffset);
            }
        }

        public void PopOrder() => PopMeshProperties();
        public void PushOrder(int order)
        {
            var prop = GetCurrentMeshProperties();
            prop.Order = order;
            PushMeshProperties(ref prop);
        }

        public void PopMaterial() => PopMeshProperties();
        public void PushMaterial(Material material)
        {
            var prop = GetCurrentMeshProperties();
            prop.Material = material;
            PushMeshProperties(ref prop);
        }

        public void PopClipRect() => PopMeshProperties();
        public void PushClipRect(Rect rect)
        {
            var prop = GetCurrentMeshProperties();
            rect = prop.ClipRect.Enabled ? prop.ClipRect.Rect.Intersection(rect) : rect;
            prop.ClipRect.Enabled = true;
            prop.ClipRect.Rect = rect;
            PushMeshProperties(ref prop);
        }
        public void PushNoClipRect()
        {
            var prop = GetCurrentMeshProperties();
            prop.ClipRect.Enabled = false;
            PushMeshProperties(ref prop);
        }

        public MeshProperties GetCurrentMeshProperties()
        {
            return meshPropertiesStack.Peek();
        }
        
        public void PushMeshProperties(ref MeshProperties properties)
        {
            meshPropertiesStack.Push(ref properties);
            drawer.NextMesh();
            
            ApplyMeshProperties();
        }
        
        public void PopMeshProperties()
        {
            meshPropertiesStack.Pop();
            drawer.NextMesh();
            
            ApplyMeshProperties();
        }
        
        private void ApplyMeshProperties()
        {
            ref var mesh = ref drawer.GetMesh();
            ref var prop = ref meshPropertiesStack.Peek();

            mesh.Material = prop.Material;
            mesh.Order = prop.Order;
            mesh.ClipRect = prop.ClipRect;
        }
        
        private MeshProperties GetDefaultMeshProperties()
        {
            return new MeshProperties()
            {
                Order = 0,
                ClipRect = new MeshClipRect()
                {
                    Enabled = true,
                    Rect = screen
                },
                Material = material
            };
        }

        public void Rect(ImRect rect, Color32 color)
        {
            Rect(rect, color, defaultTexScaleOffset);
        }

        public void Rect(ImRect rect, Color32 color, Vector4 texScaleOffset)
        {
            drawer.Color = color;
            drawer.ScaleOffset = texScaleOffset;
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