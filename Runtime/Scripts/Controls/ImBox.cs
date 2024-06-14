using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImBox
    {
        public static void Box(this ImGui gui, in ImRect rect, in ImBoxStyle style)
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
    }
}