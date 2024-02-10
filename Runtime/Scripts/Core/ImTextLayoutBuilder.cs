using System;
using Imui.Rendering;
using UnityEngine;

namespace Imui.Core
{
    [Serializable]
    public struct ImTextLayoutSettings
    {
        public float Size;
        [Range(0.0f, 1.0f)]
        public float AlignX;
        [Range(0.0f, 1.0f)] 
        public float AlignY;
    }

    public class ImTextLayoutBuilder
    {
        private const char NEW_LINE = '\n';
                
        private static TextDrawer.Layout SharedLayout = new()
        {
            Lines = new TextDrawer.Line[128]
        };
        
        private readonly TextDrawer drawer;
        
        public ImTextLayoutBuilder(TextDrawer drawer)
        {
            this.drawer = drawer;
        }

        public ref readonly TextDrawer.Layout BuildLayout(ReadOnlySpan<char> text, ImTextLayoutSettings settings, float width, float height)
        {
            FillLayout(text, settings, width, height, ref SharedLayout);
            return ref SharedLayout;
        }
        
        public void FillLayout(ReadOnlySpan<char> text, ImTextLayoutSettings settings, float width, float height, ref TextDrawer.Layout layout)
        {
            layout.LinesCount = 0;
            layout.Scale = settings.Size / drawer.FontRenderSize;
            layout.OffsetX = width * settings.AlignX;
            
            if (text.Length == 0)
            {
                return;
            }

            var maxLineWidth = 0f;
            var lineHeight = drawer.LineHeight * layout.Scale;
            var lineWidth = 0f;
            var lineStart = 0;
            var textLength = text.Length;
            var fontAsset = drawer.FontAsset;
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

                if (c == NEW_LINE || (width >= 0 && lineWidth > 0 && (lineWidth + advance) > width))
                {
                    ref var line = ref layout.Lines[layout.LinesCount];

                    line.Width = lineWidth;
                    line.Start = lineStart;
                    line.Count = i - lineStart;
                    line.OffsetX = (width - lineWidth) * settings.AlignX;

                    if (line.Width > maxLineWidth)
                    {
                        maxLineWidth = line.Width;
                    }

                    lineWidth = advance;
                    lineStart = i;

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
            if (text.Length > lineStart)
            {
                ref var line = ref layout.Lines[layout.LinesCount];

                line.Width = lineWidth;
                line.Start = lineStart;
                line.Count = textLength - lineStart;
                line.OffsetX = (width - lineWidth) * settings.AlignX;

                if (line.Width > maxLineWidth)
                {
                    maxLineWidth = line.Width;
                }

                layout.OffsetX = Mathf.Min(line.OffsetX, layout.OffsetX);
                layout.LinesCount++;
            }

            layout.Width = maxLineWidth;
            layout.Height = lineHeight * layout.LinesCount;
            layout.OffsetY = -(height - layout.LinesCount * lineHeight) * settings.AlignY;
        }
    }
}