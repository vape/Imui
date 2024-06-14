using Imui.Core;
using Imui.Controls.Styling;

namespace Imui.Controls
{
    // TODO (artem-s): allow to add title bar to panel
    public static class ImPanel
    {
        public static ImPanelStyle Style = ImPanelStyle.Default;
        
        public static void BeginPanel(this ImGui gui, in ImRect rect)
        {
            gui.Box(in rect, Style.Box);
            gui.RegisterRaycastTarget(rect);
            
            gui.Layout.Push(ImAxis.Vertical, rect.WithPadding(Style.Padding));
            gui.Canvas.PushRectMask(rect.WithPadding(Style.Box.BorderWidth), Style.Box.BorderRadius);
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