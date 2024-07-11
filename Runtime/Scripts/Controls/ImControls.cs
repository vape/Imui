using Imui.Controls.Styling;
using Imui.Core;

namespace Imui.Controls
{
    // TODO (artem-s): implement table layout helper
    public static class ImControls
    {
        public static ImRect GetRowRect(ImGui gui, ImSize size)
        {
            return size.Type switch
            {
                ImSizeType.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            var padding =
                ImTheme.Active.Controls.Padding.Bottom +
                ImTheme.Active.Controls.Padding.Top;

            return gui.TextDrawer.GetLineHeight(ImTheme.Active.Controls.TextSize) + padding;
        }
        
        public static void AddSpacingIfLayoutFrameNotEmpty(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            if (frame.Size.x != 0 || frame.Size.y != 0)
            {
                gui.AddSpacing();
            }
        }
        
        public static void AddSpacing(this ImGui gui)
        {
            gui.Layout.AddSpace(ImTheme.Active.Controls.ControlsSpacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }

        public static void BeginIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(ImTheme.Active.Controls.Indent);
        }

        public static void EndIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(-ImTheme.Active.Controls.Indent);
        }
    }
    
    public struct ImControlsStyle
    {
        public ImPadding Padding;
        public float TextSize;
        public float ControlsSpacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Indent;
    }
}