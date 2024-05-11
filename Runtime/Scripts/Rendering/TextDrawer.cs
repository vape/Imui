using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

namespace Imui.Rendering
{
    public class TextDrawer : IDisposable
    {
        private const string ASCII = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private const char NEW_LINE = '\n';

        private const float FONT_ATLAS_W = 1024;
        private const float FONT_ATLAS_H = 1024;
        
        private const int FONT_ATLAS_PADDING = 2;
        
        public struct Line
        {
            public int Start;
            public int Count;
            public float OffsetX;
            public float Width;
        }
    
        public struct Layout
        {
            public float Size;
            public float Scale;
            public float OffsetX;
            public float OffsetY;
            public float Width;
            public float Height;
            public Line[] Lines;
            public int LinesCount;
            public float LineHeight;
        }

        private static Layout SharedLayout = new()
        {
            Lines = new Line[128]
        };

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
        
        private readonly MeshBuffer buffer;
        
        private bool disposed;

        public TextDrawer(MeshBuffer buffer)
        {
            this.buffer = buffer;
        }

        public void LoadFont(Font font, float? size = null)
        {
            UnloadFont();
            
            fontAsset = FontAsset.CreateFontAsset(font, (int)(size ?? font.fontSize), FONT_ATLAS_PADDING, GlyphRenderMode.SMOOTH,
                (int)FONT_ATLAS_W, (int)FONT_ATLAS_H, enableMultiAtlasSupport: false);
            fontAsset.TryAddCharacters(ASCII);
            fontAsset.atlasTexture.Apply();

            renderSize = fontAsset.faceInfo.pointSize;
            lineHeight = fontAsset.faceInfo.lineHeight;
            descentLine = fontAsset.faceInfo.descentLine;
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

        public float GetLineHeight(float size)
        {
            return FontLineHeight * (size / FontRenderSize);
        }

        public float GetCharacterWidth(char c, float size)
        {
            if (!fontAsset.characterLookupTable.TryGetValue(c, out var character))
            {
                return 0f;
            }
            
            var scale = size / FontRenderSize;
            return character.glyph.metrics.horizontalAdvance * scale;
        }

        public void AddText(ReadOnlySpan<char> text, float scale, float x, float y)
        {
            Profiler.BeginSample("TextDrawer.AddText");
            
            var ct = fontAsset.characterLookupTable;
            var lh = lineHeight * scale;
            
            y -= lh;
            
            buffer.EnsureVerticesCapacity(buffer.VerticesCount + text.Length * 4);
            buffer.EnsureIndicesCapacity(buffer.IndicesCount + text.Length * 6);

            for (int i = 0; i < text.Length; ++i)
            {
                var c = text[i];
                if (!ct.TryGetValue(c, out var info))
                {
                    continue;
                }
                
                x += AddGlyphQuad(info.glyph, x , y, scale);
            }
            
            Profiler.EndSample();
        }
        
        public void AddTextWithLayout(ReadOnlySpan<char> text, in Layout layout, float x, float y)
        {
            Profiler.BeginSample("TextDrawer.AddTextWithLayout");
            
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
                    // TODO (artem-s): handle tabs
                    if (!ct.TryGetValue(c, out var charInfo))
                    {
                        continue;
                    }

                    x += AddGlyphQuad(charInfo.glyph, x + line.OffsetX, y + layout.OffsetY, layout.Scale);
                }

                y -= lh;
                x = sx;
            }
            
            Profiler.EndSample();
        }
        
#if IMUI_DEBUG
        private void AddControlGlyphQuad(char c, float px, float py, float scale)
        {
            var ct = fontAsset.characterLookupTable;
            
            switch (c)
            {
                case '\n':
                    px += AddGlyphQuad(ct['\\'].glyph, px, py, scale);
                    px += AddGlyphQuad(ct['n'].glyph, px, py, scale);
                    break;
                case '\t':
                    px += AddGlyphQuad(ct['\\'].glyph, px, py, scale);
                    px += AddGlyphQuad(ct['t'].glyph, px, py, scale);
                    break;
                
            }
        }
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float AddGlyphQuad(Glyph glyph, float px, float py, float scale)
        {
            var rect = glyph.glyphRect;
            var x = rect.x;
            var y = rect.y;
            var h = rect.height;
            var w = rect.width;
            var metrics = glyph.metrics;

            var glyphWidth = w * scale;
            var glyphHeight = h * scale;
            var verticalOffset = (metrics.horizontalBearingY - h - descentLine) * scale;
            var horizontalOffset = metrics.horizontalBearingX * scale;

            var vc = buffer.VerticesCount;
            var ic = buffer.IndicesCount;
            
            ref var v0 = ref buffer.Vertices[vc + 0];
            v0.Position.x = px + horizontalOffset;
            v0.Position.y = py + verticalOffset;
            v0.Position.z = Depth;
            v0.Color = Color;
            v0.UV.x = x / FONT_ATLAS_W;
            v0.UV.y = y / FONT_ATLAS_H;
            v0.Atlas = MeshDrawer.FONT_ATLAS_ID;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = px + horizontalOffset;
            v1.Position.y = py + glyphHeight + verticalOffset;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = x / FONT_ATLAS_W;
            v1.UV.y = (y + h) / FONT_ATLAS_H;
            v1.Atlas = MeshDrawer.FONT_ATLAS_ID;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = px + glyphWidth + horizontalOffset;
            v2.Position.y = py + glyphHeight + verticalOffset;
            v2.Position.z = Depth;
            v2.Color = Color;
            v2.UV.x = (x + w) / FONT_ATLAS_W;
            v2.UV.y = (y + h) / FONT_ATLAS_H;
            v2.Atlas = MeshDrawer.FONT_ATLAS_ID;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = px + glyphWidth + horizontalOffset;
            v3.Position.y = py + verticalOffset;
            v3.Position.z = Depth;
            v3.Color = Color;
            v3.UV.x = (x + w) / FONT_ATLAS_W;
            v3.UV.y = (y) / FONT_ATLAS_H;
            v3.Atlas = MeshDrawer.FONT_ATLAS_ID;
            
            buffer.Indices[ic + 0] = vc + 0;
            buffer.Indices[ic + 1] = vc + 1;
            buffer.Indices[ic + 2] = vc + 2;
            buffer.Indices[ic + 3] = vc + 2;
            buffer.Indices[ic + 4] = vc + 3;
            buffer.Indices[ic + 5] = vc + 0;

            buffer.AddVertices(4);
            buffer.AddIndices(6);
            
            return metrics.horizontalAdvance * scale;
        }
        
        public ref readonly Layout BuildTempLayout(in ReadOnlySpan<char> text, float width, float height, float alignX, float alignY, float size)
        {
            FillLayout(text, width, height, alignX, alignY, size, ref SharedLayout);
            return ref SharedLayout;
        }
        
        public void FillLayout(ReadOnlySpan<char> text, float width, float height, float alignX, float alignY, float size, ref Layout layout)
        {
            layout.LinesCount = 0;
            layout.Scale = size / FontRenderSize;
            layout.OffsetX = width * alignX;
            layout.Width = 0;
            layout.Height = 0;
            layout.Size = size;
            layout.LineHeight = this.lineHeight * layout.Scale;
            
            if (text.Length == 0)
            {
                return;
            }

            var maxLineWidth = 0f;
            var lineWidth = 0f;
            var lineStart = 0;
            var lineHeight = layout.LineHeight;
            var textLength = text.Length;
            var fontAsset = FontAsset;
            var charsTable = fontAsset.characterLookupTable;

            for (int i = 0; i < textLength; ++i)
            {
                var c = text[i];

                if (!charsTable.TryGetValue(c, out var charInfo))
                {
                    if (fontAsset.HasCharacter(c, tryAddCharacter: true))
                    {
                        charInfo = charsTable[c];
                    }
                    else
                    {
                        continue;
                    }
                }

                var advance = charInfo.glyph.metrics.horizontalAdvance * layout.Scale;
                var newLine = c == NEW_LINE;
                
                if (newLine || (width > 0 && lineWidth > 0 && (lineWidth + advance) > width))
                {
                    ref var line = ref layout.Lines[layout.LinesCount];
                    
                    line.Width = lineWidth;
                    line.Start = lineStart;
                    line.Count = i - lineStart + (newLine ? 1 : 0);
                    line.OffsetX = (width - lineWidth) * alignX;

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
                }
                else
                {
                    lineWidth += advance;
                }
            }

            // TODO (artem-s): merge with rest of the loop
            if (text.Length > lineStart || text[lineStart - 1] == NEW_LINE)
            {
                ref var line = ref layout.Lines[layout.LinesCount];

                line.Width = lineWidth;
                line.Start = lineStart;
                line.Count = textLength - lineStart;
                line.OffsetX = (width - lineWidth) * alignX;

                if (line.Width > maxLineWidth)
                {
                    maxLineWidth = line.Width;
                }

                layout.OffsetX = Mathf.Min(line.OffsetX, layout.OffsetX);
                layout.LinesCount++;
            }

            layout.Width = maxLineWidth;
            layout.Height = lineHeight * layout.LinesCount;
            layout.OffsetY = -(height - layout.LinesCount * lineHeight) * alignY;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            
            UnityEngine.Object.Destroy(fontAsset);
            fontAsset = null;
            
            disposed = true;
        }
    }
}