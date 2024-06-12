using System;
using Imui.Rendering;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    public struct ImCanvasSettings
    {
        public Material Material;
        public ImMeshClipRect ClipRect;
        public ImMeshMaskRect MaskRect;
        public Texture MainTex;
        public Texture FontTex;
        public int Order;
    }
    
    public partial class ImCanvas : IDisposable
    {
        public const int DEFAULT_ORDER = 0;
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;
        
        private const int MESH_SETTINGS_CAPACITY = 32;

        private const float LINE_THICKNESS_THRESHOLD = 0.01f;
        
        private const string SHADER_NAME = "imui_default";
        
        private static Texture2D CreateDefaultTexture()
        {
            var pixels = new Color32[DEFAULT_TEX_W * DEFAULT_TEX_H];
            Array.Fill(pixels, Color.white);
            
            var texture = new Texture2D(DEFAULT_TEX_W, DEFAULT_TEX_H, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        public ImRect ScreenRect => new ImRect(Vector2.zero, ScreenSize);
        public Vector2 ScreenSize => screenSize;

        public int DrawingDepth = 0;
        public Vector4 TexScaleOffset = new(1, 1, 0, 0);
        
        private Shader shader;
        private Material material;
        private Texture2D defaultTexture;
        private ImDynamicArray<ImCanvasSettings> settingsStack;
        private Vector2 screenSize;
        private bool disposed;
        
        private readonly ImMeshDrawer meshDrawer;
        private readonly ImTextDrawer textDrawer;
        
        public ImCanvas(ImMeshDrawer meshDrawer, ImTextDrawer textDrawer)
        {
            this.meshDrawer = meshDrawer;
            this.textDrawer = textDrawer;
            
            shader = Resources.Load<Shader>(SHADER_NAME);
            material = new Material(shader);
            defaultTexture = CreateDefaultTexture();
            settingsStack = new ImDynamicArray<ImCanvasSettings>(MESH_SETTINGS_CAPACITY);
        }
        
        public void SetScreen(Vector2 screenSize)
        {
            this.screenSize = screenSize;
        }

        public void Clear()
        {
            meshDrawer.Clear();
        }

        public void PushSettings(in ImCanvasSettings settings)
        {
            settingsStack.Push(in settings);
            meshDrawer.NextMesh();
            
            ApplySettings();
        }
        
        public void PopSettings()
        {
            settingsStack.Pop();

            if (settingsStack.Count > 0)
            {
                meshDrawer.NextMesh();
                ApplySettings();
            }
        }

        internal ref readonly ImCanvasSettings GetActiveSettings()
        {
            return ref settingsStack.Peek();
        }
        
        public ImCanvasSettings GetActiveSettingsCopy()
        {
            return settingsStack.Peek();
        }
        
        public ImCanvasSettings CreateDefaultSettings()
        {
            return new ImCanvasSettings()
            {
                Order = DEFAULT_ORDER,
                ClipRect = new ImMeshClipRect()
                {
                    Enabled = true,
                    Rect = new Rect(Vector2.zero, screenSize)
                },
                Material = material,
                MainTex = defaultTexture,
                FontTex = textDrawer.FontAtlas
            };
        }

        private void ApplySettings()
        {
            ref var mesh = ref meshDrawer.GetMesh();
            ref var settings = ref settingsStack.Peek();

            mesh.FontTex = settings.FontTex;
            mesh.MainTex = settings.MainTex;
            mesh.Material = settings.Material;
            mesh.Order = settings.Order;
            mesh.ClipRect = settings.ClipRect;
            mesh.MaskRect = settings.MaskRect;
        }

        public bool Cull(ImRect rect)
        {
            var r = (Rect)rect;
            
            ref var settings = ref settingsStack.Peek();
            if (settings.ClipRect.Enabled && !settings.ClipRect.Rect.Overlaps(r))
            {
                return true;
            }

            if (settings.MaskRect.Enabled && !settings.MaskRect.Rect.Overlaps(r))
            {
                return true;
            }

            return !ScreenRect.Overlaps(rect);
        }

        public void Circle(Vector2 position, float radius, Color32 color)
        {
            var rect = new ImRect(position.x - radius, position.y - radius, radius * 2, radius * 2);
            Ellipse(rect, color);
        }

        public void Ellipse(ImRect rect, Color32 color)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Ellipse(rect);
            ConvexFill(path, color);
        }
        
        public void Rect(ImRect rect, Color32 color)
        {
            if (Cull(rect))
            {
                return;
            }
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddQuad(rect.X, rect.Y, rect.W, rect.H);
        }
        
        public void Rect(ImRect rect, Color32 color, ImRectRadius radius)
        {
            if (Cull(rect))
            {
                return;
            }
            
            var path = ImShapes.Rect(rect, radius);
            ConvexFill(path, color);
        }

        public void RectOutline(ImRect rect, Color32 color, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect) || thickness < LINE_THICKNESS_THRESHOLD)
            {
                return;
            }
            
            var path = ImShapes.Rect(rect, radius);
            LineMiter(path, color, true, thickness, bias);
        }

        public void RectWithOutline(ImRect rect, Color32 color, Color32 outlineColor, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Rect(rect, radius);
            ConvexFill(path, color);
            
            if (thickness >= LINE_THICKNESS_THRESHOLD)
            {
                LineMiter(path, outlineColor, true, thickness, bias);
            }
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, Vector2 position, float size)
        {
            textDrawer.Color = color;
            textDrawer.Depth = DrawingDepth;
            textDrawer.AddText(text, size / textDrawer.FontRenderSize, position.x, position.y);
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, Vector2 position, in ImTextLayout layout)
        {
            var rect = new ImRect(position.x + layout.OffsetX, position.y - layout.Height + layout.OffsetY, layout.Width, layout.Height);
            if (Cull(rect))
            {
                return;
            }
            
            textDrawer.Color = color;
            textDrawer.Depth = DrawingDepth;
            textDrawer.AddTextWithLayout(text, in layout, position.x, position.y);
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.Align.X, settings.Align.Y, settings.Size);
            Text(text, color, rect.TopLeft, in layout);
        }
        
        public void Text(in ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings, out ImRect textRect)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.Align.X, settings.Align.Y, settings.Size);
            
            textRect = new ImRect(
                rect.X + layout.OffsetX, 
                rect.Y + layout.OffsetY - (layout.Height - rect.H), 
                layout.Width, 
                layout.Height);
            
            Text(text, color, rect.TopLeft, in layout);
        }

        public void Line(in ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLine(in path, closed, thickness, bias, 1.0f - bias);
        }

        public void LineMiter(in ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLineMiter(in path, closed, thickness, bias, 1.0f - bias);
        }

        public void ConvexFill(in ReadOnlySpan<Vector2> points, Color32 color)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddFilledConvexMesh(in points);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            Resources.UnloadAsset(shader);
            UnityEngine.Object.Destroy(material);
            UnityEngine.Object.Destroy(defaultTexture);
            
            disposed = true;
        }
    }
}