using System;
using Imui.Core;
using Imui.Controls.Styling;

namespace Imui.Controls
{
    // TODO (artem-s): allow to add title bar to panel
    public static class ImPanel
    {
        public static void BeginPanel(this ImGui gui, ImRect rect)
        {
            gui.Box(rect, ImTheme.Active.Panel.Box);
            gui.RegisterRaycastTarget(rect);

            var layoutRect = rect.WithPadding(ImTheme.Active.Panel.Padding);
            var maskRect = rect.WithPadding(ImTheme.Active.Panel.Box.BorderWidth);
            
            gui.Layout.Push(ImAxis.Vertical, layoutRect);
            gui.Canvas.PushRectMask(maskRect, ImTheme.Active.Panel.Box.BorderRadius);
            gui.Canvas.PushClipRect(maskRect); // need this to properly handle clicking outside drawing area
        }

        public static void EndPanel(this ImGui gui)
        {
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();
            gui.Layout.Pop();
        }

        public static float GetEnclosingHeight(float contentHeight)
        {
            return contentHeight + ImTheme.Active.Panel.Padding.Vertical;
        }
    }

    [Serializable]
    public struct ImPanelStyle
    {
        public ImPadding Padding;
        public ImBoxStyle Box;
    }
}