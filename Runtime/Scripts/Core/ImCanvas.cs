using System;
using System.Runtime.CompilerServices;
using Imui.Rendering;
using Imui.Style;
using Imui.Utility;
using UnityEngine;

namespace Imui.Core
{
    /// <summary>
    /// Contextual canvas settings
    /// </summary>
    public struct ImCanvasSettings
    {
        /// <summary>
        /// Material used to draw anything by canvas
        /// </summary>
        public Material Material;

        /// <summary>
        /// Hardware clip rect
        /// </summary>
        public ImMeshClipRect ClipRect;

        /// <summary>
        /// Shader masked rectangle with variable radius
        /// </summary>
        public ImMeshMaskRect MaskRect;

        /// <summary>
        /// Main texture
        /// </summary>
        public Texture MainTex;

        /// <summary>
        /// Font texture
        /// </summary>
        public Texture FontTex;

        /// <summary>
        /// Order or drawing for current draw call
        /// </summary>
        public int Order;

        /// <summary>
        /// Inverse color multiplier, from 0 to 1 (color * (1 - InvColorMul))
        /// </summary>
        public float InvColorMul;
    }

    /// <summary>
    /// Builtin canvas texture
    /// </summary>
    public enum ImCanvasBuiltinTex
    {
        /// <summary>
        /// Plain white square
        /// </summary>
        Primary,

        /// <summary>
        /// Checkrboard pattern
        /// </summary>
        Checkerboard,
        
        /// <summary>
        /// Texture for AA lines (1-2px)
        /// </summary>
        AALine
    }

    public partial class ImCanvas: IDisposable
    {
        public const int DEFAULT_ORDER = 0;

        private const int SETTINGS_CAPACITY = 32;

        private const float LINE_THICKNESS_THRESHOLD = 0.01f;

        public const int PRIM_TEX_X = 0;
        public const int PRIM_TEX_Y = 0;
        public const int PRIM_TEX_W = 4;
        public const int PRIM_TEX_H = 4;
        public const int PRIM_TEX_PADDING = 1;

        public const int CB_TEX_X = PRIM_TEX_W;
        public const int CB_TEX_Y = 0;
        public const int CB_TEX_W = 32;
        public const int CB_TEX_H = 32;
        public const int CB_TEX_S = 8;
        
        public const int AALINE_TEX_X = CB_TEX_X + CB_TEX_W;
        public const int AALINE_TEX_Y = 0;
        public const int AALINE_TEX_W = 3;
        public const int AALINE_TEX_H = 9;

        public const int MAIN_ATLAS_W = 64;
        public const int MAIN_ATLAS_H = 32;

        /// <summary>
        /// Generates the main texture atlas used by the canvas.
        /// </summary>
        /// <returns>A new Texture2D representing the main atlas.</returns>
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

            var semiTransparent33 = Color.white.WithAlpha(0.33f);
            var semiTransparent66 = Color.white.WithAlpha(0.66f);
            
            for (int x = AALINE_TEX_X; x < AALINE_TEX_X + AALINE_TEX_W; ++x)
            {
                pixels[(AALINE_TEX_Y + 0) * MAIN_ATLAS_W + x] = Color.clear;
                pixels[(AALINE_TEX_Y + 1) * MAIN_ATLAS_W + x] = semiTransparent33;
                pixels[(AALINE_TEX_Y + 2) * MAIN_ATLAS_W + x] = semiTransparent66;
                pixels[(AALINE_TEX_Y + 3) * MAIN_ATLAS_W + x] = Color.white;
                pixels[(AALINE_TEX_Y + 4) * MAIN_ATLAS_W + x] = Color.white;
                pixels[(AALINE_TEX_Y + 5) * MAIN_ATLAS_W + x] = Color.white;
                pixels[(AALINE_TEX_Y + 6) * MAIN_ATLAS_W + x] = semiTransparent66;
                pixels[(AALINE_TEX_Y + 7) * MAIN_ATLAS_W + x] = semiTransparent33;
                pixels[(AALINE_TEX_Y + 8) * MAIN_ATLAS_W + x] = Color.clear;
            }

            var texture = new Texture2D(MAIN_ATLAS_W, MAIN_ATLAS_H, TextureFormat.RGBA32, false, false);
            texture.filterMode = FilterMode.Point;
            texture.SetPixels32(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Gets the texture scale and offset for the specified built-in texture type.
        /// </summary>
        /// <param name="texture">The type of built-in texture to retrieve the scale and offset for.</param>
        /// <returns>A Vector4 representing the texture scale (xy) and offset (zw).</returns>
        public static Vector4 GetTexScaleOffsetFor(ImCanvasBuiltinTex texture)
        {
            switch (texture)
            {
                case ImCanvasBuiltinTex.Checkerboard:
                    return new Vector4(
                        CB_TEX_W / (float)MAIN_ATLAS_W, CB_TEX_H / (float)MAIN_ATLAS_H,
                        CB_TEX_X / (float)MAIN_ATLAS_W, CB_TEX_Y / (float)MAIN_ATLAS_H);
                case ImCanvasBuiltinTex.AALine:
                    return new Vector4(
                        AALINE_TEX_W / (float)MAIN_ATLAS_W, AALINE_TEX_H / (float)MAIN_ATLAS_H,
                        AALINE_TEX_X / (float)MAIN_ATLAS_W, AALINE_TEX_Y / (float)MAIN_ATLAS_H);
                default:
                    return new Vector4(
                        (PRIM_TEX_W - PRIM_TEX_PADDING * 2) / (float)MAIN_ATLAS_W, (PRIM_TEX_H - PRIM_TEX_PADDING * 2) / (float)MAIN_ATLAS_H,
                        (PRIM_TEX_X + PRIM_TEX_PADDING) / (float)MAIN_ATLAS_W, (PRIM_TEX_Y + PRIM_TEX_PADDING) / (float)MAIN_ATLAS_H);
            }
        }

        /// <summary>
        /// Controls how settings should be applied.
        /// </summary>
        private struct SettingsPref
        {
            public readonly bool RequiresNextMesh;

            public SettingsPref(bool requiresNextMesh = true)
            {
                RequiresNextMesh = requiresNextMesh;
            }
        }

        /// <summary>
        /// Line drawing settings
        /// </summary>
        private struct LineSettings
        {
            public Vector4 TexScaleOffset;
            public float ExtraScale;
        }

        /// <summary>
        /// Whole screen rect
        /// </summary>
        public ImRect ScreenRect => new ImRect(Vector2.zero, ScreenSize);

        /// <summary>
        /// Safe screen rect
        /// </summary>
        public ImRect SafeScreenRect =>
            new ImRect(
                SafeAreaPadding.Left,
                SafeAreaPadding.Bottom,
                ScreenSize.x - SafeAreaPadding.Left - SafeAreaPadding.Right,
                ScreenSize.y - SafeAreaPadding.Bottom - SafeAreaPadding.Top);

        /// <summary>
        /// Size of screen
        /// </summary>
        public Vector2 ScreenSize => screenSize;

        /// <summary>
        /// Screen scale
        /// </summary>
        public float ScreenScale => screenScale;

        /// <summary>
        /// Z coordinate for all generated meshes
        /// </summary>
        public int DrawingDepth = 0;

        /// <summary>
        /// Safe area padding for each side of the screen
        /// </summary>
        public ImAABB SafeAreaPadding;

        private Shader shader;
        private Material material;
        private Texture2D defaultTexture;
        private ImDynamicArray<ImCanvasSettings> settingsStack;
        private ImDynamicArray<SettingsPref> settingsPrefStack;
        private Vector2 screenSize;
        private float screenScale;

        private bool disposed;
        
        private ImAABB cullingBounds;
        private ImTextClipRect textClipRect;
        private Vector4 texScaleOffset;
        private LineSettings[] lineSettings;

        private readonly ImMeshDrawer meshDrawer;
        private readonly ImTextDrawer textDrawer;
        private readonly ImArena arena;

        public ImCanvas(ImMeshDrawer meshDrawer, ImTextDrawer textDrawer, ImArena arena)
        {
            ImShapes.BuildTables();

            this.meshDrawer = meshDrawer;
            this.textDrawer = textDrawer;
            this.arena = arena;

            shader = Resources.Load<Shader>("Imui/imui_default");
            material = new Material(shader);
            defaultTexture = CreateMainAtlas();
            settingsStack = new ImDynamicArray<ImCanvasSettings>(SETTINGS_CAPACITY);
            settingsPrefStack = new ImDynamicArray<SettingsPref>(SETTINGS_CAPACITY);
            
            SetTexScaleOffset(GetTexScaleOffsetFor(ImCanvasBuiltinTex.Primary));

            var aaLine = new LineSettings() { ExtraScale = 0.4f, TexScaleOffset = GetTexScaleOffsetFor(ImCanvasBuiltinTex.AALine) };
            var defLine = new LineSettings() { ExtraScale = 0.0f, TexScaleOffset = GetTexScaleOffsetFor(ImCanvasBuiltinTex.Primary) };
            
            lineSettings = new[] { aaLine, aaLine, aaLine, defLine };
        }

        /// <summary>
        /// Sets the size and scale of the virtual canvas screen.
        /// </summary>
        /// <param name="screenSize">The size of the screen.</param>
        /// <param name="screenScale">The scale of the screen.</param>
        /// <param name="safeAreaPadding">Default safe area padding.</param>
        public void ConfigureScreen(Vector2 screenSize, float screenScale, ImAABB safeAreaPadding = default)
        {
            this.screenSize = screenSize;
            this.screenScale = screenScale;
            
            SafeAreaPadding = safeAreaPadding;
        }

        /// <summary>
        /// Clears the mesh drawer. Does not actually release nor zero memory.
        /// </summary>
        public void Clear()
        {
            meshDrawer.Clear();
        }

        /// <summary>
        /// Pushes canvas settings onto the settings stack and applies them.
        /// Starts a new draw call if required.
        /// </summary>
        /// <param name="settings">The canvas settings to push.</param>
        public void PushSettings(in ImCanvasSettings settings)
        {
            var pref = new SettingsPref(true);

            settingsStack.Push(in settings);
            settingsPrefStack.Push(in pref);

            if (pref.RequiresNextMesh)
            {
                meshDrawer.NextMesh();
                ApplySettings();
            }
        }

        /// <summary>
        /// Pushes canvas settings onto the settings stack and applies them.
        /// Additional option can control how settings should be applied.
        /// </summary>
        /// <param name="settings">The canvas settings to push.</param>
        /// <param name="pref">How to apply given settings.</param>
        private void PushSettings(in ImCanvasSettings settings, in SettingsPref pref)
        {
            settingsStack.Push(in settings);
            settingsPrefStack.Push(in pref);

            if (pref.RequiresNextMesh)
            {
                meshDrawer.NextMesh();
            }

            ApplySettings();
        }

        /// <summary>
        /// Pops canvas settings from the stack and applies the previous settings.
        /// Starts a new draw call if required.
        /// </summary>
        public void PopSettings()
        {
            var pref = settingsPrefStack.Pop();
            settingsStack.Pop();

            if (settingsStack.Count > 0)
            {
                if (pref.RequiresNextMesh)
                {
                    meshDrawer.NextMesh();
                }

                ApplySettings();
            }
        }

        /// <summary>
        /// Retrieves the current texture scale and offset used by mesh drawer.
        /// </summary>
        /// <returns>A Vector4 representing the current texture scale and offset.</returns>
        public Vector4 GetTexScaleOffset()
        {
            return texScaleOffset;
        }

        /// <summary>
        /// Sets the current texture scale and offset used by mesh drawer.
        /// </summary>
        /// <param name="scaleOffset">The texture scale and offset to set.</param>
        public void SetTexScaleOffset(Vector4 scaleOffset)
        {
            texScaleOffset = scaleOffset;
        }

        /// <summary>
        /// Retrieves a reference to currently active canvas settings.
        /// </summary>
        /// <returns>Reference to active <see cref="ImCanvasSettings"/>.</returns>
        internal ref readonly ImCanvasSettings GetActiveSettings()
        {
            return ref settingsStack.Peek();
        }

        /// <summary>
        /// Retrieves a copy of the currently active canvas settings.
        /// </summary>
        /// <returns>A copy of the current <see cref="ImCanvasSettings"/>.</returns>
        public ImCanvasSettings GetActiveSettingsCopy()
        {
            return settingsStack.Peek();
        }

        /// <summary>
        /// Creates a set of default canvas settings.
        /// </summary>
        /// <returns>A new instance of <see cref="ImCanvasSettings"/> with default values.</returns>
        public ImCanvasSettings CreateDefaultSettings()
        {
            return new ImCanvasSettings()
            {
                Order = DEFAULT_ORDER,
                ClipRect = new ImMeshClipRect() { Enabled = true, Rect = new Rect(Vector2.zero, screenSize) },
                Material = material,
                MainTex = defaultTexture,
                FontTex = textDrawer.FontAtlas
            };
        }

        /// <summary>
        /// Applies topmost settings on the stack.
        /// </summary>
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

            cullingBounds = CalculateCullingBounds();
            textClipRect = new ImTextClipRect(cullingBounds.Left, cullingBounds.Right, cullingBounds.Top, cullingBounds.Bottom);
        }

        /// <summary>
        /// Given active screen, clip rect and mask rect, calculates final rectangle for culling on CPU.
        /// </summary>
        /// <returns>Cull rect</returns>
        private ImAABB CalculateCullingBounds()
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

            return new ImAABB(result);
        }

        /// <summary>
        /// Gets culling bounds.
        /// </summary>
        /// <returns>Culling bounds.</returns>
        public ref readonly ImAABB GetCullingBounds()
        {
            return ref cullingBounds;
        }

        /// <summary>
        /// Checks if the specified rectangle is outside the clipping area.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns>True if the rectangle should be culled; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Cull(in ImRect rect)
        {
            return !cullingBounds.Overlaps(in rect);
        }

        /// <summary>
        /// Draws a circle at the specified position with the given radius and color.
        /// </summary>
        /// <param name="position">The center position of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        public void Circle(Vector2 position, float radius, Color32 color)
        {
            var rect = new ImRect(position.x - radius, position.y - radius, radius * 2, radius * 2);
            Ellipse(rect, color);
        }

        /// <summary>
        /// Draws a circle with an outline.
        /// </summary>
        /// <param name="position">The center position of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The fill color of the circle.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1. 0 - inside, 1 - outside.</param>
        public void CircleWithOutline(Vector2 position, float radius, Color32 color, Color32 outlineColor, float thickness, float bias = 0.0f)
        {
            var rect = new ImRect(position.x - radius, position.y - radius, radius * 2, radius * 2);
            EllipseWithOutline(rect, color, outlineColor, thickness, bias);
        }

        /// <summary>
        /// Draws an ellipse within the specified rectangle.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        public void Ellipse(ImRect rect, Color32 color)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Ellipse(arena, rect);
            ConvexFill(path, color);
        }

        /// <summary>
        /// Draws an ellipse with an outline.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the ellipse.</param>
        /// <param name="color">The fill color of the ellipse.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1. 0 - inside, 1 - outside.</param>
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
                Line(path, outlineColor, true, thickness, bias);
            }
        }

        /// <summary>
        /// Draws a rectangle with the specified color.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
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

        /// <summary>
        /// Draws a rectangle with rounded corners.
        /// </summary>
        /// <param name="rect">The bounding rectangle of the shape.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="radius">The corner radius of the rectangle.</param>
        public void Rect(ImRect rect, Color32 color, ImRectRadius radius)
        {
            if (Cull(rect))
            {
                return;
            }

            var path = ImShapes.Rect(arena, rect, radius);
            ConvexFillTextured(path, color, in rect);
        }

        /// <summary>
        /// Draws an outline around a rectangular area.
        /// </summary>
        /// <param name="rect">The rectangle bounds.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="radius">Corner radius for rounded rectangles.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1. 0 - inside, 1 - outside.</param>
        public void RectOutline(ImRect rect, Color32 color, float thickness, ImRectRadius radius = default, float bias = 0.0f)
        {
            if (Cull(rect) || thickness < LINE_THICKNESS_THRESHOLD)
            {
                return;
            }

            var path = ImShapes.Rect(arena, rect, radius);
            Line(path, color, true, thickness, bias);
        }

        /// <summary>
        /// Draws a filled rectangle with an outline.
        /// </summary>
        /// <param name="rect">The rectangle bounds.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="outlineColor">The color of the outline.</param>
        /// <param name="thickness">The thickness of the outline.</param>
        /// <param name="radius">Corner radius for rounded rectangles.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1. 0 - inside, 1 - outside.</param>
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
                Line(path, outlineColor, true, thickness, bias);
            }
        }

        /// <summary>
        /// Renders text at a specific position with given layout.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="position">The position to render the text.</param>
        /// <param name="layout">The layout for the text.</param>
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

        /// <summary>
        /// Automatically generates layout and renders text within a rectangle using specific text settings.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="rect">The bounding rectangle for the text.</param>
        /// <param name="settings">The settings for text alignment, size, and wrapping.</param>
        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(
                text, rect.W, rect.H,
                settings.Align.X, settings.Align.Y, settings.Size, settings.Wrap, settings.Overflow);
            Text(text, color, rect.TopLeft, in layout);
        }

        /// <summary>
        /// Automatically generates layout and renders text within a rectangle with configurable options.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="rect">The bounding rectangle for the text.</param>
        /// <param name="size">The font size of the text.</param>
        /// <param name="alignX">Horizontal alignment (default 0.5f for center).</param>
        /// <param name="alignY">Vertical alignment (default 0.5f for center).</param>
        /// <param name="wrap">Specifies whether text wrapping is enabled.</param>
        /// <param name="overflow">Specifies how text overflows its bounds.</param>
        public void Text(ReadOnlySpan<char> text,
                         Color32 color,
                         ImRect rect,
                         float size,
                         float alignX = 0.5f,
                         float alignY = 0.5f,
                         bool wrap = false,
                         ImTextOverflow overflow = ImTextOverflow.Overflow)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(
                text, rect.W, rect.H,
                alignX, alignY, size, wrap, overflow);
            Text(text, color, rect.TopLeft, in layout);
        }

        /// <summary>
        /// Automatically generates layout and renders text and outputs the rectangle bounds of the rendered text.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="color">The color of the text.</param>
        /// <param name="rect">The bounding rectangle for the text.</param>
        /// <param name="settings">The settings for text alignment, size, and wrapping.</param>
        /// <param name="textRect">Outputs the rectangle bounds of the rendered text.</param>
        public void Text(ReadOnlySpan<char> text, Color32 color, ImRect rect, in ImTextSettings settings, out ImRect textRect)
        {
            ref readonly var layout = ref textDrawer.BuildTempLayout(
                text, rect.W, rect.H,
                settings.Align.X, settings.Align.Y, settings.Size, settings.Wrap, settings.Overflow);

            textRect = new ImRect(
                rect.X + layout.OffsetX,
                rect.Y + layout.OffsetY - (layout.Height - rect.H),
                layout.Width,
                layout.Height);

            Text(text, color, rect.TopLeft, in layout);
        }

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        /// <param name="p0">The starting point of the line.</param>
        /// <param name="p1">The ending point of the line.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">The thickness of the line.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1.</param>
        public void Line(Vector2 p0, Vector2 p1, Color32 color, float thickness, float bias = 0.5f)
        {
            // TODO (artem-s): add clipping

            if (thickness <= 0)
            {
                return;
            }

            thickness = Mathf.Max(thickness, thickness / screenScale);
            bias = Mathf.Clamp01(bias);
            
            var settings = (int)thickness >= lineSettings.Length ? lineSettings[^1] : lineSettings[(int)thickness];
            var outer = bias + settings.ExtraScale;
            var inner = 1 - bias + settings.ExtraScale; 

            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = settings.TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLine(stackalloc Vector2[2] { p0, p1 }, false, thickness, outer, inner);
        }

        /// <summary>
        /// Draws a line following a given path of points.
        /// </summary>
        /// <param name="path">The series of points defining the line path.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="closed">Indicates whether the line should be closed into a loop.</param>
        /// <param name="thickness">The thickness of the line.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1.</param>
        public void Line(ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }

            thickness = Mathf.Max(thickness, thickness / screenScale);
            bias = Mathf.Clamp01(bias);

            var settings = (int)thickness >= lineSettings.Length ? lineSettings[^1] : lineSettings[(int)thickness];
            var outer = bias + settings.ExtraScale;
            var inner = 1 - bias + settings.ExtraScale; 

            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = settings.TexScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLine(path, closed, thickness, outer, inner);
        }

        /// <summary>
        /// Draws a line with miter joints following a given path of points.
        /// </summary>
        /// <param name="path">The series of points defining the line path.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="closed">Indicates whether the line should be closed into a loop.</param>
        /// <param name="thickness">The thickness of the line.</param>
        /// <param name="bias">Controls where thickness grows. From 0 to 1.</param>
        public void LineMiter(ReadOnlySpan<Vector2> path, Color32 color, bool closed, float thickness, float bias = 0.5f)
        {
            if (thickness <= 0)
            {
                return;
            }

            thickness = Mathf.Max(thickness, thickness / screenScale);
            bias = Mathf.Clamp01(bias);
            
            var outer = bias;
            var inner = 1 - bias; 

            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddLineMiter(path, closed, thickness, outer, inner);
        }

        /// <summary>
        /// Fills a convex polygon defined by a series of points with a solid color.
        /// </summary>
        /// <param name="points">The series of points defining the convex polygon.</param>
        /// <param name="color">The fill color.</param>
        public void ConvexFill(ReadOnlySpan<Vector2> points, Color32 color)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddFilledConvexMesh(points);
        }

        /// <summary>
        /// Fills a convex polygon defined by a series of points with a textured fill.
        /// </summary>
        /// <param name="points">The series of points defining the convex polygon.</param>
        /// <param name="color">The fill color.</param>
        /// <param name="bounds">The texture bounds for the fill.</param>
        public void ConvexFillTextured(ReadOnlySpan<Vector2> points, Color32 color, in ImRect bounds)
        {
            meshDrawer.Color = color;
            meshDrawer.ScaleOffset = texScaleOffset;
            meshDrawer.Atlas = ImMeshDrawer.MAIN_TEX_ID;
            meshDrawer.Depth = DrawingDepth;
            meshDrawer.AddFilledConvexMeshTextured(points, bounds.X, bounds.Y, bounds.W, bounds.H);
        }

        /// <summary>
        /// Releases all resources used by canvas.
        /// </summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            Resources.UnloadAsset(shader);
            ImUnityUtility.Destroy(material);
            ImUnityUtility.Destroy(defaultTexture);

            disposed = true;
        }
    }
}