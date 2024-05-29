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

            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(Style.Padding));
            gui.Canvas.PushRectMask(rect.WithPadding(Style.FrameWidth), Style.CornerRadius.GetMax());
            gui.BeginScrollable();
        }

        public static void EndPanel(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Canvas.PopRectMask();
            gui.Layout.Pop();
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

        public float GetHeight(float contentHeight)
        {
            return contentHeight + Padding.Vertical;
        }
        
        public ImRect GetContentRect(ImRect popupRect)
        {
            return popupRect.WithPadding(Padding);
        }
    }
}