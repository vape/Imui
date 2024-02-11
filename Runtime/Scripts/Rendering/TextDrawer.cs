using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.TextCore.Text;

namespace Imui.Rendering
{
    public class TextDrawer : IDisposable
    {
        private const string ASCII = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        
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
            public float Scale;
            public float OffsetX;
            public float OffsetY;
            public float Width;
            public float Height;
            public Line[] Lines;
            public int LinesCount;
        }

        public Texture2D FontAtlas => fontAsset.atlasTexture;
        public FontAsset FontAsset => fontAsset;

        public float Depth;
        public float UVZ;
        public Color32 Color;

        public float FontRenderSize => renderSize;
        public float LineHeight => lineHeight;
        
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

        public void AddText(ReadOnlySpan<char> text, float scale, float x, float y)
        {
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
        }
        
        public void AddTextWithLayout(ReadOnlySpan<char> text, in Layout layout, float x, float y)
        {
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
                    if (!ct.TryGetValue(c, out var charInfo))
                    {
                        continue;
                    }

                    x += AddGlyphQuad(charInfo.glyph, x + line.OffsetX, y + layout.OffsetY, layout.Scale);
                }

                y -= lh;
                x = sx;
            }
        }
        
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
            v0.UV.z = UVZ;

            ref var v1 = ref buffer.Vertices[vc + 1];
            v1.Position.x = px + horizontalOffset;
            v1.Position.y = py + glyphHeight + verticalOffset;
            v1.Position.z = Depth;
            v1.Color = Color;
            v1.UV.x = x / FONT_ATLAS_W;
            v1.UV.y = (y + h) / FONT_ATLAS_H;
            v1.UV.z = UVZ;
            
            ref var v2 = ref buffer.Vertices[vc + 2];
            v2.Position.x = px + glyphWidth + horizontalOffset;
            v2.Position.y = py + glyphHeight + verticalOffset;
            v2.Position.z = Depth;
            v2.Color = Color;
            v2.UV.x = (x + w) / FONT_ATLAS_W;
            v2.UV.y = (y + h) / FONT_ATLAS_H;
            v2.UV.z = UVZ;

            ref var v3 = ref buffer.Vertices[vc + 3];
            v3.Position.x = px + glyphWidth + horizontalOffset;
            v3.Position.y = py + verticalOffset;
            v3.Position.z = Depth;
            v3.Color = Color;
            v3.UV.x = (x + w) / FONT_ATLAS_W;
            v3.UV.y = (y) / FONT_ATLAS_H;
            v3.UV.z = UVZ;
            
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