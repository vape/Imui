using Imui.Core;
using Imui.Styling;
using Imui.Utility;

namespace Imui.Controls
{
    public static class ImPanel
    {
        public static ImPanelStyle Style = ImPanelStyle.Default;
        
        public static void BeginPanel(this ImGui gui, in ImRect rect)
        {
            gui.DrawBox(in rect, Style.Box);

            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(Style.Padding));
            gui.Canvas.PushRectMask(rect.WithPadding(Style.Box.BorderWidth), Style.Box.BorderRadius.GetMax());
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
            Box = new ImBoxStyle
            {
                BackColor = ImColors.White,
                BorderColor = ImColors.Black,
                BorderWidth = 1,
                BorderRadius = 3
            },
            Padding = 4f
        };

        public ImBoxStyle Box;
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