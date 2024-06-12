using System;
using Imui.Rendering;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Profiling;

namespace Imui.Core
{
    public struct ImCanvasSettings
    {
        public Material Material;
        public ImMeshClipRect ClipRect;
        public ImMeshMaskRect MaskRect;
        public int Order;
        public Texture MainTex;
        public Texture FontTex;
    }
    
    public partial class ImCanvas : IDisposable
    {
        public const int DEFAULT_ORDER = 0;
        
        // TODO (artem-s): allow to change drawing depth
        private const int DEFAULT_DEPTH = 0;
        
        private const int DEFAULT_TEX_W = 4;
        private const int DEFAULT_TEX_H = 4;
        
        private const int MESH_SETTINGS_CAPACITY = 32;
        private const int TEMP_POINTS_BUFFER_CAPACITY = 1024;

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
        public Vector4 DefaultTexScaleOffset => defaultTexScaleOffset;
        
        private Shader shader;
        private Material material;
        private Texture2D defaultTexture;
        private ImDynamicArray<ImCanvasSettings> settingsStack;
        private Vector2 screenSize;
        private ImResizeableBuffer<Vector2> tempPointsBuffer;
        private bool disposed;
        
        private readonly Vector4 defaultTexScaleOffset;
        private readonly ImMeshDrawer meshDrawer;
        private readonly ImTextDrawer textDrawer;
        
        public ImCanvas(ImMeshDrawer meshDrawer, ImTextDrawer textDrawer)
        {
            this.meshDrawer = meshDrawer;
            this.textDrawer = textDrawer;
            
            shader = Resources.Load<Shader>(SHADER_NAME);
            material = new Material(shader);
            defaultTexture = CreateDefaultTexture();
            defaultTexScaleOffset = new Vector4(1, 1, 0, 0);
            settingsStack = new ImDynamicArray<ImCanvasSettings>(MESH_SETTINGS_CAPACITY);
            tempPointsBuffer = new ImResizeableBuffer<Vector2>(TEMP_POINTS_BUFFER_CAPACITY);
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
        
        public void Rect(ImRect rect, Color32 color, Vector4 texScaleOffset)
        {
            if (Cull(rect))
            {
                return;
            }
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddQuad(rect.X, rect.Y, rect.W, rect.H);
        }
        
        public void Rect(ImRect rect, Color32 color, ImRectRadius radius = default)
        {
            if (Cull(rect))
            {
                return;
            }
            
            var path = GenerateRectOutline(rect, radius);
            ConvexFill(path, color);
        }

        public void RectOutline(ImRect rect, Color32 color, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect) || thickness < LINE_THICKNESS_THRESHOLD)
            {
                return;
            }
            
            var path = GenerateRectOutline(rect, radius);
            LineMiter(path, color, true, thickness, bias);
        }

        public void RectWithOutline(ImRect rect, Color32 backColor, Color32 outlineColor, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = GenerateRectOutline(rect, radius);
            ConvexFill(path, backColor);
            
            if (thickness >= LINE_THICKNESS_THRESHOLD)
            {
                LineMiter(path, outlineColor, true, thickness, bias);
            }
        }

        public void Text(in ReadOnlySpan<char> text, Color32 color, Vector2 position, float size)
        {
            textDrawer.Color = color;
            textDrawer.Depth = DEFAULT_DEPTH;
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
            textDrawer.Depth = DEFAULT_DEPTH;
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

        public void LineSimple(in ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DEFAULT_DEPTH;
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
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddLineMiter(in path, closed, thickness, bias, 1.0f - bias);
        }

        public void ConvexFill(in ReadOnlySpan<Vector2> points, Color32 color)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = defaultTexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DEFAULT_DEPTH;
            meshDrawer.AddFilledConvexMesh(in points);
        }

        public Span<Vector2> GenerateRectOutline(ImRect rect, ImRectRadius radius)
        {
            radius.Clamp(Mathf.Min(rect.W, rect.H) / 2.0f);

            var segments = ImMeshDrawer.CalculateSegmentsCount(radius.GetMax());
            var span = tempPointsBuffer.AsSpan((segments + 1) * 4);
            
            GenerateRectOutline(span, rect, radius, segments);
            
            return span;
        }
        
        public void GenerateRectOutline(Span<Vector2> buffer, ImRect rect, ImRectRadius radius, int segments)
        {
            const float PI = Mathf.PI;
            const float HALF_PI = PI / 2;
         
            Profiler.BeginSample("ImCanvas.GenerateRectOutlinePath");
            
            var p = 0;
            var step = (1f / segments) * HALF_PI;
            
            var cx = rect.X + rect.W - radius.BottomRight;
            var cy = rect.Y + radius.BottomRight;
            buffer[p].x = cx + Mathf.Cos(PI + HALF_PI) * radius.BottomRight;
            buffer[p].y = cy + Mathf.Sin(PI + HALF_PI) * radius.BottomRight;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.BottomRight;
                buffer[p].y = cy + Mathf.Sin(a) * radius.BottomRight;
                p++;
            }
            
            cx = rect.X + rect.W - radius.TopRight;
            cy = rect.Y + rect.H - radius.TopRight;
            buffer[p].x = cx + Mathf.Cos(0) * radius.TopRight;
            buffer[p].y = cy + Mathf.Sin(0) * radius.TopRight;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = 0 + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.TopRight;
                buffer[p].y = cy + Mathf.Sin(a) * radius.TopRight;
                p++;
            }
            
            cx = rect.X + radius.TopLeft;
            cy = rect.Y + rect.H - radius.TopLeft;
            buffer[p].x = cx + Mathf.Cos(HALF_PI) * radius.TopLeft;
            buffer[p].y = cy + Mathf.Sin(HALF_PI) * radius.TopLeft;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = HALF_PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.TopLeft;
                buffer[p].y = cy + Mathf.Sin(a) * radius.TopLeft;
                p++;
            }
                        
            cx = rect.X + radius.BottomLeft;
            cy = rect.Y + radius.BottomLeft;
            buffer[p].x = cx + Mathf.Cos(PI) * radius.BottomLeft;
            buffer[p].y = cy + Mathf.Sin(PI) * radius.BottomLeft;
            p++;
            
            for (int i = 0; i < segments; ++i)
            {
                var a = PI + step * (i + 1);
                buffer[p].x = cx + Mathf.Cos(a) * radius.BottomLeft;
                buffer[p].y = cy + Mathf.Sin(a) * radius.BottomLeft;
                p++;
            }
            
            Profiler.EndSample();
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