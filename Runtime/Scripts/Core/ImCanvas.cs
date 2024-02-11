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
            public MeshMaskRect MaskRect;
            public int Order;
        }

        // TODO (artem-s): allow to change drawing depth
        private const int DEFAULT_DEPTH = 0;
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;
                
        private const int MAIN_ATLAS_W = 1024;
        private const int MAIN_ATLAS_H = 1024;
        
        private const float MAIN_ATLAS_IDX = 0;
        private const float FONT_ATLAS_IDX = 1;
        
        private const int MESH_PROP_CAPACITY = 16;
        
        private const string SHADER_NAME = "imui_default";
        
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int FontTexId = Shader.PropertyToID("_FontTex");
        
        private static readonly Texture2D DefaultTexture;

        static ImCanvas()
        {
            var pixels = new Color32[DEFAULT_TEX_W * DEFAULT_TEX_H];
            Array.Fill(pixels, Color.white);
            
            DefaultTexture = new Texture2D(DEFAULT_TEX_W, DEFAULT_TEX_H, TextureFormat.RGBA32, false);
            DefaultTexture.SetPixels32(pixels);
            DefaultTexture.Apply();
        }

        public Vector2 ScreenSize => screenSize;
        public ImTextLayoutBuilder TextLayoutBuilder => textLayoutBuilder;
        
        private Shader shader;
        private Material material;
        private TextureAtlas atlas;
        private DynamicArray<TextureInfo> texturesInfo;
        private DynamicArray<MeshProperties> meshPropertiesStack;
        private Vector2 frameSize;
        private Vector2 screenSize;
        private ImTextLayoutBuilder textLayoutBuilder;
        private bool disposed;
        
        private readonly Vector4 defaultTexScaleOffset;
        private readonly MeshDrawer meshDrawer;
        private readonly TextDrawer textDrawer;
        
        public ImCanvas(MeshDrawer meshDrawer, TextDrawer textDrawer)
        {
            this.meshDrawer = meshDrawer;
            this.textDrawer = textDrawer;
            
            shader = Resources.Load<Shader>(SHADER_NAME);
            material = new Material(shader);
            atlas = new TextureAtlas(MAIN_ATLAS_W, MAIN_ATLAS_H);
            texturesInfo = new DynamicArray<TextureInfo>(capacity: 64);
            meshPropertiesStack = new DynamicArray<MeshProperties>(MESH_PROP_CAPACITY);
            textLayoutBuilder = new ImTextLayoutBuilder(textDrawer);

            defaultTexScaleOffset = AddToAtlas(DefaultTexture);
            defaultTexScaleOffset.x *= 0.5f;
            defaultTexScaleOffset.y *= 0.5f;
            defaultTexScaleOffset.z += defaultTexScaleOffset.x / 2.0f;
            defaultTexScaleOffset.w += defaultTexScaleOffset.y / 2.0f;
        }
        
        public void SetScreen(Vector2 size, float scale)
        {
            frameSize = size;
            screenSize = size / scale;
        }

        public void Begin()
        {
            material.SetTexture(MainTexId, atlas.AtlasTexture);
            material.SetTexture(FontTexId, textDrawer.FontAtlas);
            
            meshDrawer.Clear();

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
        
        public void SetupAtlas(CommandBuffer cmd)
        {
            for (int i = 0; i < texturesInfo.Count; ++i)
            {
                ref var tex = ref texturesInfo.Array[i];
                atlas.Blit(cmd, tex.Texture, ref tex.ScaleOffset);
            }
        }
        
        public Vector4 GetRectMask(Rect rect) => GetRectMask(ImRect.FromUnityRect(rect));
        public Vector4 GetRectMask(ImRect rect)
        {
            var hw = rect.W / 2f;
            var hh = rect.H / 2f;
            
            return new Vector4(rect.X + hw, rect.Y + hh, hw, hh);
        }
        
        public void PopOrder() => PopMeshProperties();
        public void PushOrder(int order)
        {
            var prop = GetCurrentMeshProperties();
            prop.Order = order;
            PushMeshProperties(ref prop);
        }

        public void PopMaterial() => PopMeshProperties();
        public void PushMaterial(Material mat)
        {
            var prop = GetCurrentMeshProperties();
            prop.Material = mat;
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

        public void PopRectMask()
        {
            PopMeshProperties();
        }

        public void PushRectMask(Rect rect, float radius)
        {
            PushRectMask(GetRectMask(rect), radius);
        }
        
        public void PushRectMask(ImRect rect, float radius)
        {
            PushRectMask(GetRectMask(rect), radius);
        }
        
        public void PushRectMask(Vector4 rect, float radius)
        {
            var prop = GetCurrentMeshProperties();
            prop.MaskRect.Enabled = true;
            prop.MaskRect.Rect = rect;
            prop.MaskRect.Radius = radius;
            PushMeshProperties(ref prop);
        }

        public MeshProperties GetCurrentMeshProperties()
        {
            return meshPropertiesStack.Peek();
        }
        
        public void PushMeshProperties(ref MeshProperties properties)
        {
            meshPropertiesStack.Push(ref properties);
            meshDrawer.NextMesh();
            
            ApplyMeshProperties();
        }
        
        public void PopMeshProperties()
        {
            meshPropertiesStack.Pop();
            meshDrawer.NextMesh();
            
            ApplyMeshProperties();
        }
        
        private void ApplyMeshProperties()
        {
            ref var mesh = ref meshDrawer.GetMesh();
            ref var prop = ref meshPropertiesStack.Peek();

            mesh.Material = prop.Material;
            mesh.Order = prop.Order;
            mesh.ClipRect = prop.ClipRect;
            mesh.MaskRect = prop.MaskRect;
        }
        
        private MeshProperties GetDefaultMeshProperties()
        {
            return new MeshProperties()
            {
                Order = 0,
                ClipRect = new MeshClipRect()
                {
                    Enabled = true,
                    Rect = new Rect(Vector2.zero, screenSize)
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
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddQuad(rect.X, rect.Y, rect.W, rect.H);
        }

        public void Rect(ImRect rect, Color32 color, float cornerRadius)
        {
            Rect(rect, color, defaultTexScaleOffset, cornerRadius);
        }

        public void Rect(ImRect rect, Color32 color, Vector4 texScaleOffset, float cornerRadius)
        {
            cornerRadius = Mathf.Min(cornerRadius, Mathf.Min(rect.W, rect.H) / 2.0f);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddRoundCornersRect(
                (Vector4)rect, 
                cornerRadius, cornerRadius, cornerRadius, cornerRadius, 
                meshDrawer.GetSegmentsCount(cornerRadius));
        }

        public void RectOutline(ImRect rect, Color32 color, float thickness, float cornerRadius)
        {
            const float EPSILON = 0.0001f;
            
            cornerRadius = Mathf.Min(cornerRadius, (Mathf.Min(rect.W, rect.H) / 2.0f) - EPSILON);

            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddRoundCornersRectOutline(
                (Vector4)rect, thickness, 
                cornerRadius, cornerRadius, cornerRadius, cornerRadius, 
                meshDrawer.GetSegmentsCount(cornerRadius));
        }

        public void Text(ReadOnlySpan<char> text, Color32 color, Vector2 position, float size)
        {
            textDrawer.Color = color;
            textDrawer.UVZ = FONT_ATLAS_IDX;
            textDrawer.Depth = DEFAULT_DEPTH;
            textDrawer.AddText(text, size / textDrawer.FontRenderSize, position.x, position.y);
        }

        public void Text(ReadOnlySpan<char> text, Color32 color, Vector2 position, in TextDrawer.Layout layout)
        {
            textDrawer.Color = color;
            textDrawer.UVZ = FONT_ATLAS_IDX;
            textDrawer.Depth = DEFAULT_DEPTH;
            textDrawer.AddTextWithLayout(text, in layout, position.x, position.y);
        }

        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextLayoutSettings settings)
        {
            ref readonly var layout = ref textLayoutBuilder.BuildLayout(text, settings, rect.W, rect.H);
            Text(text, color, rect.TopLeft, in layout);
        }
        
        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextLayoutSettings settings, out ImRect textRect)
        {
            ref readonly var layout = ref textLayoutBuilder.BuildLayout(text, settings, rect.W, rect.H);
            
            textRect = new ImRect(
                rect.X + layout.OffsetX, 
                rect.Y + layout.OffsetY - (layout.Height - rect.H), 
                layout.Width, 
                layout.Height);
            
            Text(text, color, rect.TopLeft, in layout);
        }

        public void Line(in ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness)
        {
            meshDrawer.Color = color;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddLine(in path, closed, thickness);
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