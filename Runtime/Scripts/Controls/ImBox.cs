using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImBox
    {
        public static void Box(this ImGui gui, ImRect rect, in ImBoxStyle style)
        {
            gui.Canvas.RectWithOutline(rect, style.BackColor, style.BorderColor, style.BorderWidth, style.BorderRadius);
        }
    }
    
        
    [Serializable]
    public struct ImBoxStyle
    {
        public Color32 BackColor;
        public Color32 FrontColor;
        public Color32 BorderColor;
        public float BorderWidth;
        public ImRectRadius BorderRadius;

        public ImBoxStyle Apply(ImControlAdjacency adjacency)
        {
            if ((adjacency & ImControlAdjacency.Left) != 0)
            {
                BorderRadius.BottomRight = 0;
                BorderRadius.TopRight = 0;
            }

            if ((adjacency & ImControlAdjacency.Right) != 0)
            {
                BorderRadius.BottomLeft = 0;
                BorderRadius.TopLeft = 0;
            }

            return this;
        }
    }
}