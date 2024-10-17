using Imui.Core;
using Imui.Style;
using UnityEngine;

namespace Imui.Controls
{
    // TODO (artem-s): implement table layout helper
    public static class ImControls
    {
        public static ImRect AddRowRect(ImGui gui, ImSize size)
        {
            return size.Mode switch
            {
                ImSizeMode.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeightFromFontSize(gui.Style.Layout.TextSize) + gui.Style.Layout.ExtraRowHeight;
        }

        public static float GetRowsHeightWithSpacing(this ImGui gui, int rows)
        {
            return Mathf.Max(0, gui.GetRowHeight() * rows + gui.Style.Layout.Spacing * (rows - 1));
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
            gui.Layout.AddSpace(gui.Style.Layout.Spacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }

        public static void BeginIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(gui.Style.Layout.Indent);
        }

        public static void EndIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(-gui.Style.Layout.Indent);
        }
    }
}