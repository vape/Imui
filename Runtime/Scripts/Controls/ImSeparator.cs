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
            
            bool vertical;
            float width;
            float height;
            
            if (gui.Layout.Axis == ImAxis.Horizontal)
            {
                vertical = true;
                width = gui.Style.Separator.Thickness;
                height = gui.Layout.GetContentRect().H;
            }
            else
            {
                vertical = false;
                width = gui.GetLayoutWidth();
                height = gui.Style.Separator.Thickness;
            }
            
            Separator(gui, gui.AddLayoutRect(width, height), vertical);
        }
        
        public static unsafe void Separator(this ImGui gui, ImRect rect, bool horizontal)
        {
            Vector2 p0;
            Vector2 p1;
            
            if (horizontal)
            {
                p0 = rect.BottomCenter;
                p1 = rect.TopCenter;
            }
            else
            {
                p0 = rect.LeftCenter;
                p1 = rect.RightCenter;
            }

            Span<Vector2> path = stackalloc Vector2[2] { p0, p1 };
            gui.Canvas.Line(path, gui.Style.Separator.Color, false, gui.Style.Separator.Thickness);
        }
    }
}