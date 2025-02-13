using System;
using Imui.Core;
using Imui.Rendering;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImSeparator
    {
        public static void Separator(this ImGui gui)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            Separator(gui, gui.AddLayoutRect(gui.GetLayoutWidth(), gui.Style.Separator.Thickness));
        }

        public static void Separator(this ImGui gui, ReadOnlySpan<char> label)
        {
            gui.AddSpacingIfLayoutFrameNotEmpty();

            Separator(gui, label, gui.AddLayoutRect(gui.GetLayoutWidth(), gui.Style.Layout.TextSize));
        }

        public static void Separator(ImGui gui, ImRect rect)
        {
            if (gui.Canvas.Cull(rect))
            {
                return;
            }

            var p0 = rect.LeftCenter;
            var p1 = rect.RightCenter;

            gui.Canvas.Line(p0, p1, gui.Style.Separator.Color, gui.Style.Separator.Thickness);
        }

        public static void Separator(ImGui gui, ReadOnlySpan<char> label, ImRect rect)
        {
            if (gui.Canvas.Cull(rect))
            {
                return;
            }

            var fontSize = Mathf.Min(gui.Style.Layout.TextSize, gui.TextDrawer.GetFontSizeFromLineHeight(rect.H));
            var textSettings = new ImTextSettings(fontSize, gui.Style.Separator.TextAlignment, overflow: gui.Style.Separator.TextOverflow);
            var textRectSize = gui.MeasureTextSize(label, in textSettings, rect.Size);
            var start = Mathf.Max(rect.X, rect.X + (rect.W - textRectSize.x) * gui.Style.Separator.TextAlignment.X);
            var end = Mathf.Min(start + textRectSize.x, rect.Right);
            var textRect = new ImRect(start, rect.Y, end - start, rect.H);

            var p0 = rect.LeftCenter;
            var p1 = new Vector2(start - gui.Style.Separator.TextMargin.Left, rect.Center.y);
            var p2 = new Vector2(end + gui.Style.Separator.TextMargin.Right, rect.Center.y);
            var p3 = rect.RightCenter;

            if (p0.x < p1.x)
            {
                gui.Canvas.Line(p0, p1, gui.Style.Separator.Color, gui.Style.Separator.Thickness);
            }

            if (p2.x < p3.x)
            {
                gui.Canvas.Line(p2, p3, gui.Style.Separator.Color, gui.Style.Separator.Thickness);
            }

            gui.Text(label, textSettings, gui.Style.Separator.TextColor, textRect);
        }
    }
}