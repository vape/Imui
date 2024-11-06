using System;
using System.Runtime.CompilerServices;
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
        public float InvColorMul;
    }

    public enum ImCanvasBuiltinTex
    {
        Primary,
        Checkerboard
    }
    
    public partial class ImCanvas : IDisposable
    {
        public const int DEFAULT_ORDER = 0;
        
        private const int MESH_SETTINGS_CAPACITY = 32;

        private const float LINE_THICKNESS_THRESHOLD = 0.01f;
        
        private const string SHADER_NAME = "imui_default";
        
        public const int PRIM_TEX_X = 0;
        public const int PRIM_TEX_Y = 0;
        public const int PRIM_TEX_W = 4;
        public const int PRIM_TEX_H = 4;

        public const int CB_TEX_X = PRIM_TEX_W;
        public const int CB_TEX_Y = 0;
        public const int CB_TEX_W = 32;
        public const int CB_TEX_H = 32;
        public const int CB_TEX_S = 8;
        
        public const int MAIN_ATLAS_W = 64;
        public const int MAIN_ATLAS_H = 32;
        
        private static Texture2D CreateMainAtlas()
        {
            var pixels = new Color32[MAIN_ATLAS_W * MAIN_ATLAS_H];

            var dark = new Color32(128, 128, 128, 255);
            var light = new Color32(255, 255, 255, 255);
            
            for (int y = PRIM_TEX_Y; y < PRIM_TEX_Y + PRIM_TEX_H; ++y)
            {
                for (int x = PRIM_TEX_X; x < PRIM_TEX_X + PRIM_TEX_W; ++x)
                {
                    pixels[y * MAIN_ATLAS_W + x] = light;
                }
            }
            
            for (int y = CB_TEX_Y; y < CB_TEX_Y + CB_TEX_H; ++y)
            {
                for (int x = CB_TEX_X; x < CB_TEX_X + CB_TEX_W; ++x)
                {
                    pixels[y * MAIN_ATLAS_W + x] = (x / CB_TEX_S + y / CB_TEX_S) % 2 == 0 ? dark : light;
                }
            }

            var texture = new Texture2D(MAIN_ATLAS_W, MAIN_ATLAS_H, TextureFormat.RGBA32, false, false, true);
            texture.filterMode = FilterMode.Point;
            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }
        
        public static Vector4 GetTexScaleOffsetFor(ImCanvasBuiltinTex texture)
        {
            switch (texture)
            {
                case ImCanvasBuiltinTex.Checkerboard:
                    return new Vector4(
                        CB_TEX_W / (float)MAIN_ATLAS_W, CB_TEX_H / (float)MAIN_ATLAS_H,
                        CB_TEX_X / (float)MAIN_ATLAS_W, CB_TEX_Y / (float)MAIN_ATLAS_H);
                default:
                    return default;
            }
        }

        public ImRect ScreenRect => new ImRect(Vector2.zero, ScreenSize);
        public Vector2 ScreenSize => screenSize;

        public int DrawingDepth = 0;
        
        private Shader shader;
        private Material material;
        private Texture2D defaultTexture;
        private ImDynamicArray<ImCanvasSettings> settingsStack;
        private Vector2 screenSize;
        private float screenScale;
        private bool disposed;
        private ImRect cullingRect;
        private ImTextClipRect textClipRect;
        private Vector4 texScaleOffset;
        
        private readonly ImMeshDrawer meshDrawer;
        private readonly ImTextDrawer textDrawer;
        private readonly ImArena arena;
        
        public ImCanvas(ImMeshDrawer meshDrawer, ImTextDrawer textDrawer, ImArena arena)
        {
            ImShapes.BuildTables();
            
            this.meshDrawer = meshDrawer;
            this.textDrawer = textDrawer;
            this.arena = arena;
            
            shader = Resources.Load<Shader>(SHADER_NAME);
            material = new Material(shader);
            defaultTexture = CreateMainAtlas();
            settingsStack = new ImDynamicArray<ImCanvasSettings>(MESH_SETTINGS_CAPACITY);
        }
        
        public void SetScreen(Vector2 screenSize, float screenScale)
        {
            this.screenSize = screenSize;
            this.screenScale = screenScale;
        }

        public void Clear()
        {
            meshDrawer.Clear();
        }

        public void PushSettings(in ImCanvasSettings settings, bool changed = true)
        {
            settingsStack.Push(in settings);

            if (changed)
            {
                meshDrawer.NextMesh();
                ApplySettings();
            }
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

        public Vector4 GetTexScaleOffset()
        {
            return texScaleOffset;
        }
        
        public void SetTexScaleOffset(Vector4 scaleOffset)
        {
            texScaleOffset = scaleOffset;
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
            mesh.InvColorMul = settings.InvColorMul;

            cullingRect = CalculateCullRect();
            textClipRect = new ImTextClipRect(cullingRect.Left, cullingRect.Right, cullingRect.Top, cullingRect.Bottom);
        }
        
        private ImRect CalculateCullRect()
        {
            var result = ScreenRect;
            
            ref var settings = ref settingsStack.Peek();
            if (settings.ClipRect.Enabled)
            {
                result = result.Intersection((ImRect)settings.ClipRect.Rect);
            }

            if (settings.MaskRect.Enabled)
            {
                result = result.Intersection((ImRect)settings.MaskRect.Rect);
            }

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(ImRect rect)
        {
            return !cullingRect.Overlaps(rect);
        }

        public void Circle(Vector2 position, float radius, Color32 color)
        {
            var rect = new ImRect(position.x - radius, position.y - radius, radius * 2, radius * 2);
            Ellipse(rect, color);
        }
        
        public void CircleWithOutline(Vector2 position, float radius, Color32 color, Color32 outlineColor, float thickness, float bias = 0.0f)
        {
            var rect = new ImRect(position.x - radius, position.y - radius, radius * 2, radius * 2);
            EllipseWithOutline(rect, color, outlineColor, thickness, bias);
        }

        public void Ellipse(ImRect rect, Color32 color)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Ellipse(arena, rect);
            ConvexFill(path, color);
        }
        
        public void EllipseWithOutline(ImRect rect, Color32 color, Color32 outlineColor, float thickness, float bias = 0.0f)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Ellipse(arena, rect);
            ConvexFill(path, color);
            
            if (thickness >= LINE_THICKNESS_THRESHOLD)
            {
                Line(path, outlineColor, true, GetScaledLineThickness(thickness), bias);
            }
        }
        
        public void Rect(ImRect rect, Color32 color)
        {
            if (Cull(rect))
            {
                return;
            }
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddQuadTextured(rect.X, rect.Y, rect.W, rect.H);
        }
        
        public void Rect(ImRect rect, Color32 color, ImRectRadius radius)
        {
            if (Cull(rect))
            {
                return;
            }
            
            var path = ImShapes.Rect(arena, rect, radius);
            ConvexFillTextured(path, color, in rect);
        }

        public void RectOutline(ImRect rect, Color32 color, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect) || thickness < LINE_THICKNESS_THRESHOLD)
            {
                return;
            }
            
            var path = ImShapes.Rect(arena, rect, radius);
            Line(path, color, true, GetScaledLineThickness(thickness), bias);
        }

        public void RectWithOutline(ImRect rect, Color32 color, Color32 outlineColor, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Rect(arena, rect, radius);
            ConvexFillTextured(path, color, in rect);
            
            if (thickness >= LINE_THICKNESS_THRESHOLD)
            {
                Line(path, outlineColor, true, GetScaledLineThickness(thickness), bias);
            }
        }

        private float GetScaledLineThickness(float thickness)
        {
            var pixelWidth = thickness * screenScale;
            if (pixelWidth >= 1.0f)
            {
                return thickness;
            }

            return thickness + (1 - pixelWidth) / screenScale;
        }

        public void Text(ReadOnlySpan<char> text, Color32 color, Vector2 position, in ImTextLayout layout)
        {
            var rect = new ImRect(position.x + layout.OffsetX, position.y - layout.Height + layout.OffsetY, layout.Width, layout.Height);
            if (Cull(rect))
            {
                return;
            }
            
            textDrawer.Color = color;
            textDrawer.Depth = DrawingDepth;
            textDrawer.AddTextWithLayout(text, in layout, position.x, position.y, in textClipRect);
        }

        // TODO (artem-s): use few parameters instead of textsettings here and add extension that acceps text settings for convenience
        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.Align.X, settings.Align.Y, settings.Size, settings.Wrap);
            Text(text, color, rect.TopLeft, in layout);
        }
        
        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings, out ImRect textRect)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(text, rect.W, rect.H, settings.Align.X, settings.Align.Y, settings.Size, settings.Wrap);
            
            textRect = new ImRect(
                rect.X + layout.OffsetX, 
                rect.Y + layout.OffsetY - (layout.Height - rect.H), 
                layout.Width, 
                layout.Height);
            
            Text(text, color, rect.TopLeft, in layout);
        }

        public void Line(Vector2 p0, Vector2 p1, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLine(stackalloc Vector2[2] { p0, p1 }, closed, thickness, bias, 1.0f - bias);
        }
        
        public void Line(ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLine(path, closed, thickness, bias, 1.0f - bias);
        }

        public void LineMiter(ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }
            
            bias = Mathf.Clamp01(bias);
            
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLineMiter(path, closed, thickness, bias, 1.0f - bias);
        }

        public void ConvexFill(ReadOnlySpan<Vector2> points, Color32 color)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddFilledConvexMesh(points);
        }
        
        public void ConvexFillTextured(ReadOnlySpan<Vector2> points, Color32 color, in ImRect bounds)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddFilledConvexMeshTextured(points, bounds.X, bounds.Y, bounds.W, bounds.H);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            Resources.UnloadAsset(shader);
            ImObjectUtility.Destroy(material);
            ImObjectUtility.Destroy(defaultTexture);
            
            disposed = true;
        }
    }
}