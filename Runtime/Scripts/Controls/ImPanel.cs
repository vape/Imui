using Imui.Core;
using Imui.Styling;
using Imui.Utility;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImPanel
    {
        public static ImPanelStyle Style = ImPanelStyle.Default;
        
        public static void BeginPanel(this ImGui gui, ImRect rect)
        {
            gui.Canvas.RectWithOutline(rect, Style.BackColor, Style.FrameColor, Style.FrameWidth, Style.CornerRadius);

            rect.ApplyPadding(Style.FrameWidth);
            
            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(Style.Padding));
            gui.Canvas.PushRectMask(rect, Style.CornerRadius.GetMax());
            gui.BeginScrollable();
        }

        public static void EndPanel(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Canvas.PopRectMask();
            gui.Layout.Pop();
        }
        
        public static Vector2 PanelSizeFromContentSize(Vector2 size)
        {
            return new Vector2(
                size.x + Style.Padding.Left + Style.Padding.Right + Style.FrameWidth * 2,
                size.y + Style.Padding.Bottom + Style.Padding.Top + Style.FrameWidth * 2);
        }
    }

    public struct ImPanelStyle
    {
        public static readonly ImPanelStyle Default = new ImPanelStyle()
        {
            BackColor = ImColors.White,
            FrameColor = ImColors.Black,
            FrameWidth = 1,
            CornerRadius = 8,
            Padding = 4f
        };

        public Color32 BackColor;
        public Color32 FrameColor;
        public int FrameWidth;
        public ImRectRadius CornerRadius;
        public ImPadding Padding;
    }
}