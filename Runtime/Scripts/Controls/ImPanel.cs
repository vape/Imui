using Imui.Core;
using Imui.Controls.Styling;

namespace Imui.Controls
{
    // TODO (artem-s): allow to add title bar to panel
    public static class ImPanel
    {
        public static ImPanelStyle Style = ImPanelStyle.Default;
        
        public static void BeginPanel(this ImGui gui, ImRect rect)
        {
            gui.Box(rect, Style.Box);
            gui.RegisterRaycastTarget(rect);

            var layoutRect = rect.WithPadding(ImControls.Padding);
            var maskRect = rect.WithPadding(Style.Box.BorderWidth);
            
            gui.Layout.Push(ImAxis.Vertical, layoutRect);
            gui.Canvas.PushRectMask(maskRect, Style.Box.BorderRadius);
            gui.Canvas.PushClipRect(maskRect); // need this to properly handle clicking outside drawing area
            gui.BeginScrollable();
        }

        public static void EndPanel(this ImGui gui)
        {
            gui.EndScrollable();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();
            gui.Layout.Pop();
        }

        public static float GetEnclosingHeight(float contentHeight)
        {
            return contentHeight + ImControls.Padding.Vertical;
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
            }
        };

        public ImBoxStyle Box;
    }
}