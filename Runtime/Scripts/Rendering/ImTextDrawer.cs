using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Imui.Style;
using Imui.Utility;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

namespace Imui.Rendering
{
    public struct ImTextLine
    {
        public int Start;
        public int Count;
        public float OffsetX;
        public float Width;
    }

    public struct ImTextLayout
    {
        public float Size;
        public float Scale;
        public float OffsetX;
        public float OffsetY;
        public float Width;
        public float Height;
        public ImTextLine[] Lines;
        public int LinesCount;
        public float LineHeight;
    }

    public readonly struct ImTextClipRect
    {
        public readonly float Left;
        public readonly float Right;
        public readonly float Top;
        public readonly float Bottom;

        public ImTextClipRect(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }
    }

    public class ImTextDrawer : IDisposable
    {
        [Flags]
        public enum GlyphFlag : int
        {
            None = 0,
            Empty = 1
        }

        public struct GlyphData
        {
            public int x;
            public int y;
            public int w;
            public int h;
            public float bearingX;
            public float bearingY;
            public float advance;
            public GlyphFlag flag;
            public float uv0x;
            public float uv0y;
            public float uv1x;
            public float uv1y;

            public GlyphData(Glyph g)
            {
                var rect = g.glyphRect;
                var metrics = g.metrics;

                x = rect.x;
                y = rect.y;
                w = rect.width;
                h = rect.height;
                bearingX = metrics.horizontalBearingX;
                bearingY = metrics.horizontalBearingY;
                advance = metrics.horizontalAdvance;
                flag = GlyphFlag.None;
                uv0x = x / FONT_ATLAS_W;
                uv0y = y / FONT_ATLAS_H;
                uv1x = (x + w) / FONT_ATLAS_W;
                uv1y = (y + h) / FONT_ATLAS_H;
            }
        }

        private const char NEW_LINE = '\n';
        private const char SPACE = ' ';
        private const char TAB = '\t';
        private const int TAB_SPACES = 4;

        private const int GLYPH_LOOKUP_CAPACITY = 256;
        private const float FONT_ATLAS_W = 1024;
        private const float FONT_ATLAS_H = 1024;
        private const int FONT_ATLAS_PADDING = 2;

        private static ImTextLayout sharedLayout = new() { Lines = new ImTextLine[128] };

        public Texture2D FontAtlas => fontAsset.atlasTexture;
        public FontAsset FontAsset => fontAsset;

        public float Depth;
        public Color32 Color;

        public float FontRenderSize => renderSize;
        public float FontLineHeight => lineHeight;

        private FontAsset fontAsset;
        private float lineHeight;
        private float renderSize;
        private float descentLine;
        private GlyphData[] glyphsLookup;

        private readonly ImMeshBuffer buffer;

        private bool disposed;

        public ImTextDrawer(ImMeshBuffer buffer)
        {
            this.buffer = buffer;
            this.glyphsLookup = new GlyphData[256];
        }

        public void LoadFont(Font font, float? size = null)
        {
            UnloadFont();

            fontAsset = FontAsset.CreateFontAsset(font, (int)(size ?? font.fontSize), FONT_ATLAS_PADDING, GlyphRenderMode.SMOOTH_HINTED, (int)FONT_ATLAS_W,
                (int)FONT_ATLAS_H, enableMultiAtlasSupport: false);

            renderSize = fontAsset.faceInfo.pointSize;
            lineHeight = fontAsset.faceInfo.lineHeight;
            descentLine = fontAsset.faceInfo.descentLine;

            for (uint i = 0; i < glyphsLookup.Length; ++i)
            {
                if (!fontAsset.HasCharacter(i, tryAddCharacter: true))
                {
                    glyphsLookup[i] = default;
                    continue;
                }

                glyphsLookup[i] = new GlyphData(fontAsset.characterLookupTable[i].glyph);
            }

            glyphsLookup[SPACE].flag |= GlyphFlag.Empty;
            glyphsLookup[TAB].flag |= GlyphFlag.Empty;
            glyphsLookup[TAB].advance = glyphsLookup[SPACE].advance * TAB_SPACES;

            fontAsset.atlasTexture.Apply();
        }

        public void UnloadFont()
        {
            if (fontAsset == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(fontAsset);
            fontAsset = null;
        }

        public float GetLineHeightFromFontSize(float size)
        {
            return FontLineHeight * (size / FontRenderSize);
        }

        public float GetFontSizeFromLineHeight(float height)
        {
            return FontRenderSize * (height / FontLineHeight);
        }

        public float GetCharacterAdvance(char c, float size)
        {
            var scale = size / FontRenderSize;

            if (c < GLYPH_LOOKUP_CAPACITY)
            {
                return glyphsLookup[c].advance * scale;
            }

            if (fontAsset.characterLookupTable.TryGetValue(c, out var character))
            {
                return character.glyph.metrics.horizontalAdvance * scale;
            }

            return 0.0f;
        }

        public float GetCharacterAdvance(char c)
        {
            if (c < GLYPH_LOOKUP_CAPACITY)
            {
                return glyphsLookup[c].advance;
            }

            if (fontAsset.characterLookupTable.TryGetValue(c, out var character))
            {
                return character.glyph.metrics.horizontalAdvance;
            }

            return 0.0f;
        }

        public void AddTextWithLayout(ReadOnlySpan<char> text, in ImTextLayout layout, float x, float y, in ImTextClipRect clipRect)
        {
            ImProfiler.BeginSample("ImTextDrawer.AddTextWithLayout");

            var ct = fontAsset.characterLookupTable;
            var lh = lineHeight * layout.Scale;
            var sx = x;

            y -= lh;

            buffer.EnsureVerticesCapacity(buffer.VerticesCount + text.Length * 4);
            buffer.EnsureIndicesCapacity(buffer.IndicesCount + text.Length * 6);

            for (int i = 0; i < layout.LinesCount; ++i)
            {
                ref var line = ref layout.Lines[i];

                for (int k = 0; k < line.Count; ++k)
                {
                    var c = text[line.Start + k];
#if IMUI_DEBUG
                    AddControlGlyphQuad(c, x + line.OffsetX, y + layout.OffsetY, layout.Scale);
#endif

                    if (x > clipRect.Right)
                    {
                        break;
                    }

                    if (c < GLYPH_LOOKUP_CAPACITY)
                    {
                        ref var glyph = ref glyphsLookup[c];
                        if ((glyph.flag & GlyphFlag.Empty) != 0)
                        {
                            x += glyph.advance * layout.Scale;
                            continue;
                        }

                        x += AddGlyphQuad(ref glyph, x + line.OffsetX, y + layout.OffsetY, layout.Scale);
                    }
                    else
                    {
                        if (!ct.TryGetValue(c, out var character))
                        {
                            continue;
                        }

                        var glyph = new GlyphData(character.glyph);
                        x += AddGlyphQuad(ref glyph, x + line.OffsetX, y + layout.OffsetY, layout.Scale);
                    }
                }

                if (y < clipRect.Bottom)
                {
                    break;
                }

                y -= lh;
                x = sx;
            }

            ImProfiler.EndSample();
        }

#if IMUI_DEBUG
        private void AddControlGlyphQuad(char c, float px, float py, float scale)
        {
            var tmpColor = Color;
            Color.SetAlpha(0.5f * Color.GetAlpha());
            
            ref var backSlash = ref glyphsLookup['\\'];
            
            switch (c)
            {
                case '\n':
                    AddGlyphQuad(ref glyphsLookup['n'], px + AddGlyphQuad(ref backSlash, px, py, scale), py, scale);
                    break;
                case '\t':
                    AddGlyphQuad(ref glyphsLookup['t'], px + AddGlyphQuad(ref backSlash, px, py, scale), py, scale);
                    break;
            }

            Color = tmpColor;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private float AddGlyphQuad(ref GlyphData glyph, float px, float py, float scale)
        {
            var gw = scale * glyph.w;
            var gh = scale * glyph.h;
            var ox = scale * glyph.bearingX;
            var oy = scale * (glyph.bearingY - glyph.h - descentLine);

            var p0x = px + ox;
            var p0y = py + oy;
            var p1x = p0x + gw;
            var p1y = p0y + gh;

            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;

            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = p0x;
            v0.Position.y = p0y;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = glyph.uv0x;
            v0.UV.y = glyph.uv0y;
            v0.Atlas = ImMeshDrawer.FONT_TEX_ID;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = p0x;
            v1.Position.y = p1y;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = glyph.uv0x;
            v1.UV.y = glyph.uv1y;
            v1.Atlas = ImMeshDrawer.FONT_TEX_ID;

            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = p1x;
            v2.Position.y = p1y;
            v2.Position.z = Depth;
            v2.Color = Color;
            v2.UV.x = glyph.uv1x;
            v2.UV.y = glyph.uv1y;
            v2.Atlas = ImMeshDrawer.FONT_TEX_ID;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = p1x;
            v3.Position.y = p0y;
            v3.Position.z = Depth;
            v3.Color = Color;
            v3.UV.x = glyph.uv1x;
            v3.UV.y = glyph.uv0y;
            v3.Atlas = ImMeshDrawer.FONT_TEX_ID;

            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddVertices(4);
            buffer.AddIndices(6);

            return glyph.advance * scale;
        }

        public ref readonly ImTextLayout BuildTempLayout(ReadOnlySpan<char> text,
                                                         float boundsWidth,
                                                         float boundsHeight,
                                                         float alignX,
                                                         float alignY,
                                                         float size,
                                                         bool wrap)
        {
            FillLayout(text, boundsWidth, boundsHeight, alignX, alignY, size, wrap, ref sharedLayout);
            return ref sharedLayout;
        }

        public void FillLayout(ReadOnlySpan<char> text,
                               float boundsWidth,
                               float boundsHeight,
                               float alignX,
                               float alignY,
                               float size,
                               bool wrap,
                               ref ImTextLayout layout)
        {
            const float NEXT_LINE_WIDTH_THRESHOLD = 0.0001f;
            const int NO_LINE_BREAK = -1;

            layout.LinesCount = 0;
            layout.Scale = size / FontRenderSize;
            layout.OffsetX = boundsWidth * alignX;
            layout.Width = 0;
            layout.Height = 0;
            layout.Size = size;
            layout.LineHeight = lineHeight * layout.Scale;

            if (text.IsEmpty)
            {
                return;
            }
            
            ImProfiler.BeginSample("ImTextDrawer.FillLayout");

            wrap &= boundsWidth > 0;

            var maxLineWidth = 0f;
            var lineWidth = 0f;
            var lineStart = 0;
            var textLength = text.Length;
            var charsTable = fontAsset.characterLookupTable;

            var lastLineBreak = NO_LINE_BREAK;
            var lineWidthAtLineBreak = 0.0f;
            var wasBreakingChar = false;

            for (int i = 0; i < textLength; ++i)
            {
                var c = text[i];

                float a;

                if (c < GLYPH_LOOKUP_CAPACITY)
                {
                    a = glyphsLookup[c].advance;
                }
                else if (charsTable.TryGetValue(c, out var character))
                {
                    a = character.glyph.metrics.horizontalAdvance;
                }
                else if (fontAsset.HasCharacter(c, tryAddCharacter: true))
                {
                    a = charsTable[c].glyph.metrics.horizontalAdvance;
                }
                else
                {
                    continue;
                }

                if (wasBreakingChar && c != SPACE)
                {
                    lastLineBreak = i;
                    lineWidthAtLineBreak = lineWidth;
                }

                wasBreakingChar = c == SPACE;

                var advance = a * layout.Scale;
                var newLine = c == NEW_LINE;
                var jumpToNextLine = newLine;

                if (!jumpToNextLine && wrap && lineWidth > 0 && (lineWidth + advance) > (boundsWidth + NEXT_LINE_WIDTH_THRESHOLD))
                {
                    if (lastLineBreak != NO_LINE_BREAK)
                    {
                        lineWidth = lineWidthAtLineBreak;
                        i = lastLineBreak;
                    }

                    jumpToNextLine = true;
                }

                if (jumpToNextLine)
                {
                    ref var line = ref layout.Lines[layout.LinesCount];

                    line.Width = lineWidth;
                    line.Start = lineStart;
                    line.Count = i - lineStart + (newLine ? 1 : 0);
                    line.OffsetX = (boundsWidth - lineWidth) * alignX;

                    if (line.Width > maxLineWidth)
                    {
                        maxLineWidth = line.Width;
                    }

                    lineWidth = advance;
                    lineStart = i + (newLine ? 1 : 0);

                    layout.OffsetX = Mathf.Min(line.OffsetX, layout.OffsetX);
                    layout.LinesCount++;

                    if (layout.LinesCount >= layout.Lines.Length)
                    {
                        Array.Resize(ref layout.Lines, layout.Lines.Length * 2);
                    }

                    lastLineBreak = NO_LINE_BREAK;
                }
                else
                {
                    lineWidth += advance;
                }
            }

            if (text.Length > lineStart || text[lineStart - 1] == NEW_LINE)
            {
                ref var line = ref layout.Lines[layout.LinesCount];

                line.Width = lineWidth;
                line.Start = lineStart;
                line.Count = textLength - lineStart;
                line.OffsetX = (boundsWidth - lineWidth) * alignX;

                if (line.Width > maxLineWidth)
                {
                    maxLineWidth = line.Width;
                }

                layout.OffsetX = Mathf.Min(line.OffsetX, layout.OffsetX);
                layout.LinesCount++;
            }

            layout.Width = maxLineWidth;
            layout.Height = layout.LineHeight * layout.LinesCount;
            layout.OffsetY = -(boundsHeight - layout.LinesCount * layout.LineHeight) * alignY;
            
            ImProfiler.EndSample();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            ImUnityUtility.Destroy(fontAsset);
            fontAsset = null;

            disposed = true;
        }
    }
}