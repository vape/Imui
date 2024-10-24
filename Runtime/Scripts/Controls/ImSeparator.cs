using System;
using Imui.Core;
using Imui.Style;
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
            var p0 = rect.LeftCenter;
            var p1 = rect.RightCenter;
            
            gui.Canvas.Line(p0, p1, gui.Style.Separator.Color, false, gui.Style.Separator.Thickness);
        }
        
        public static void Separator(ImGui gui, ReadOnlySpan<char> label, ImRect rect)
        {
            var fontSize = Mathf.Min(gui.Style.Layout.TextSize, gui.TextDrawer.GetFontSizeFromLineHeight(rect.H));
            var textSettings = new ImTextSettings(fontSize, gui.Style.Separator.TextAlignment);
            var textRectSize = gui.MeasureTextSize(label, in textSettings, rect.Size);
            var start = rect.X + (rect.W - textRectSize.x) * gui.Style.Separator.TextAlignment.X;
            var end = start + textRectSize.x;
            var textRect = new ImRect(start, rect.Y, end - start, rect.H);

            var p0 = rect.LeftCenter;
            var p1 = new Vector2(start - gui.Style.Separator.TextMargin.Left, rect.Center.y);
            var p2 = new Vector2(end + gui.Style.Separator.TextMargin.Right, rect.Center.y);
            var p3 = rect.RightCenter;

            if (p0.x < p1.x)
            {
                gui.Canvas.Line(p0, p1, gui.Style.Separator.Color, false, gui.Style.Separator.Thickness);
            }

            if (p2.x < p3.x)
            {
                gui.Canvas.Line(p2, p3, gui.Style.Separator.Color, false, gui.Style.Separator.Thickness);
            }

            gui.Text(label, textSettings, gui.Style.Separator.TextColor, textRect);
        }
    }
}