using System;
using Imui.Controls.Styling;
using Imui.Core;
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
                width = ImTheme.Active.Separator.Thickness;
                height = gui.Layout.GetContentRect().H;
            }
            else
            {
                vertical = false;
                width = gui.GetLayoutWidth();
                height = ImTheme.Active.Separator.Thickness;
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
            gui.Canvas.Line(path, ImTheme.Active.Separator.Color, false, ImTheme.Active.Separator.Thickness);
        }
    }

    public struct ImSeparatorStyle
    {
        public float Thickness;
        public Color32 Color;
    }
}