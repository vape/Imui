using System;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImControls
    {
        public static ImControlsStyle Style = ImControlsStyle.Default;
        
        [Obsolete]
        public static float GetTextSize(this ImGui gui)
        {
            return Style.TextSize;
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeight(Style.TextSize);
        }
        
        public static void AddControlSpacing(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            if (frame.Size.x != 0 || frame.Size.y != 0)
            {
                gui.AddSpacing();
            }
        }
        
        public static void AddSpacing(this ImGui gui)
        {
            gui.Layout.AddSpace(Style.Spacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }
        
        public static void DrawBox(this ImGui gui, in ImRect rect, in ImBoxStyle style)
        {
            gui.Canvas.RectWithOutline(rect, style.BackColor, style.BorderColor, style.BorderWidth, style.BorderRadius);
        }
    }

    public struct ImControlsStyle
    {
        public static readonly ImControlsStyle Default = new ImControlsStyle()
        {
            TextSize = 26,
            Spacing = 4,
            ControlsSpacing = 2
        };
        
        public float TextSize;
        public float Spacing;
        public float ControlsSpacing;
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