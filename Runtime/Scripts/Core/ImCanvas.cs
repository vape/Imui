using System;
using Imui.Rendering;
using Imui.Rendering.Atlas;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Rendering;

namespace Imui.Core
{
    public partial class ImCanvas : IDisposable
    {
        public const int DEFAULT_ORDER = 0;
        
        // TODO (artem-s): allow to change drawing depth
        private const int DEFAULT_DEPTH = 0;
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;
                
        private const int MAIN_ATLAS_W = 1024;
        private const int MAIN_ATLAS_H = 1024;
        
        private const float MAIN_ATLAS_IDX = 0;
        private const float FONT_ATLAS_IDX = 1;
        
        private const int MESH_SETTINGS_CAPACITY = 32;
        
        private const string SHADER_NAME = "imui_default";
        
        private struct TextureInfo
        {
            public int Id;
            public Texture2D Texture;
            public Vector4 ScaleOffset;
        }

        public struct MeshSettings
        {
            public Material Material;
            public MeshClipRect ClipRect;
            public MeshMaskRect MaskRect;
            public int Order;
        }
        
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
        public Vector4 DefaultTexScaleOffset => defaultTexScaleOffset;
        
        private Shader shader;
        private Material material;
        private TextureAtlas atlas;
        private DynamicArray<TextureInfo> texturesInfo;
        private DynamicArray<MeshSettings> meshSettingsStack;
        private Vector2 frameSize;
        private Vector2 screenSize;
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
            meshSettingsStack = new DynamicArray<MeshSettings>(MESH_SETTINGS_CAPACITY);

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

        public void Clear()
        {
            material.SetTexture(MainTexId, atlas.AtlasTexture);
            material.SetTexture(FontTexId, textDrawer.FontAtlas);
            
            meshDrawer.Clear();
        }
        
        public void Setup(CommandBuffer cmd)
        {
            ImuiAssert.True(meshSettingsStack.Count == 0, "Mesh properties stack is not empty!");
            
            for (int i = 0; i < texturesInfo.Count; ++i)
            {
                ref var tex = ref texturesInfo.Array[i];
                atlas.Blit(cmd, tex.Texture, ref tex.ScaleOffset);
            }
        }

        // TODO (artem-s): maybe separate all atlas maintenance into different class?
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
        
        public void PushMeshSettings(MeshSettings settings)
        {
            PushMeshSettings(ref settings);
        }
        
        public void PushMeshSettings(ref MeshSettings settings)
        {
            meshSettingsStack.Push(ref settings);
            meshDrawer.NextMesh();
            
            ApplyMeshSettings();
        }
        
        public void PopMeshSettings()
        {
            meshSettingsStack.Pop();

            if (meshSettingsStack.Count > 0)
            {
                meshDrawer.NextMesh();
                ApplyMeshSettings();
            }
        }

        internal ref readonly MeshSettings GetActiveMeshSettingsRef()
        {
            return ref meshSettingsStack.Peek();
        }
        
        public MeshSettings GetActiveMeshSettings()
        {
            return meshSettingsStack.Peek();
        }
        
        public MeshSettings CreateDefaultMeshSettings()
        {
            return new MeshSettings()
            {
                Order = DEFAULT_ORDER,
                ClipRect = new MeshClipRect()
                {
                    Enabled = true,
                    Rect = new Rect(Vector2.zero, screenSize)
                },
                Material = material
            };
        }

        private void ApplyMeshSettings()
        {
            ref var mesh = ref meshDrawer.GetMesh();
            ref var settings = ref meshSettingsStack.Peek();

            mesh.Material = settings.Material;
            mesh.Order = settings.Order;
            mesh.ClipRect = settings.ClipRect;
            mesh.MaskRect = settings.MaskRect;
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

            var radius = new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius);
            var segments = meshDrawer.GetSegmentsCount(cornerRadius);
            
            Rect(rect, color, texScaleOffset, radius, segments);
        }

        public void Rect(ImRect rect, Color32 color, Vector4 texScaleOffset, Vector4 cornerRadius, int segments)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddRoundCornersRect(
                (Vector4)rect, 
                cornerRadius.x, cornerRadius.y, 
                cornerRadius.z, cornerRadius.w, 
                segments);
        }

        public void RectOutline(ImRect rect, Color32 color, float thickness, float cornerRadius, float bias = 0.0f)
        {
            const float EPSILON = 0.0001f;

            cornerRadius = Mathf.Min(cornerRadius, (Mathf.Min(rect.W, rect.H) / 2.0f) - EPSILON);
            
            var segments = meshDrawer.GetSegmentsCount(cornerRadius);
            Span<Vector2> path = stackalloc Vector2[(segments + 1) * 4];
            GenerateRectOutlinePath(
                in path, rect, cornerRadius, cornerRadius, cornerRadius, cornerRadius, 
                meshDrawer.GetSegmentsCount(cornerRadius));
            
            Line(path, color, true, thickness, bias);
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, Vector2 position, float size)
        {
            textDrawer.Color = color;
            textDrawer.UVZ = FONT_ATLAS_IDX;
            textDrawer.Depth = DEFAULT_DEPTH;
            textDrawer.AddText(text, size / textDrawer.FontRenderSize, position.x, position.y);
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, Vector2 position, in TextDrawer.Layout layout)
        {
            textDrawer.Color = color;
            textDrawer.UVZ = FONT_ATLAS_IDX;
            textDrawer.Depth = DEFAULT_DEPTH;
            textDrawer.AddTextWithLayout(text, in layout, position.x, position.y);
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.AlignX, settings.AlignY, settings.Size);
            Text(text, color, rect.TopLeft, in layout);
        }
        
        public void Text(in ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings, out ImRect textRect)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.AlignX, settings.AlignY, settings.Size);
            
            textRect = new ImRect(
                rect.X + layout.OffsetX, 
                rect.Y + layout.OffsetY - (layout.Height - rect.H), 
                layout.Width, 
                layout.Height);
            
            Text(text, color, rect.TopLeft, in layout);
        }

        public void Line(in ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddLine(in path, closed, thickness, bias, 1.0f - bias);
        }

        public void ConvexFill(in ReadOnlySpan<Vector2> points, Color32 color)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.UVZ = MAIN_ATLAS_IDX;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddFilledConvexMesh(in points);
        }
        
        private void GenerateRectOutlinePath(in Span<Vector2> buffer, ImRect rect, float tlr, float trr, float brr, float blr, int segments)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;
            
            var p = 0;
            var step = (1f / segments) * HALF_PI;
            
            var cx = rect.X + rect.W - brr;
            var cy = rect.Y + brr;
            buffer[p].x = cx + Mathf.Cos(PI + HALF_PI) * brr;
            buffer[p].y = cy + Mathf.Sin(PI + HALF_PI) * brr;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * brr;
                buffer[p].y = cy + Mathf.Sin(a) * brr;
                p++;
            }
            
            cx = rect.X + rect.W - trr;
            cy = rect.Y + rect.H - trr;
            buffer[p].x = cx + Mathf.Cos(0) * trr;
            buffer[p].y = cy + Mathf.Sin(0) * trr;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = 0 + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * trr;
                buffer[p].y = cy + Mathf.Sin(a) * trr;
                p++;
            }
            
            cx = rect.X + tlr;
            cy = rect.Y + rect.H - tlr;
            buffer[p].x = cx + Mathf.Cos(HALF_PI) * tlr;
            buffer[p].y = cy + Mathf.Sin(HALF_PI) * tlr;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * tlr;
                buffer[p].y = cy + Mathf.Sin(a) * tlr;
                p++;
            }
                        
            cx = rect.X + blr;
            cy = rect.Y + blr;
            buffer[p].x = cx + Mathf.Cos(PI) * blr;
            buffer[p].y = cy + Mathf.Sin(PI) * blr;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * blr;
                buffer[p].y = cy + Mathf.Sin(a) * blr;
                p++;
            }
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