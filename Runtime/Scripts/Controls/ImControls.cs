using System;
using Imui.Controls.Styling;
using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    // TODO (artem-s): implement table layout helper
    public static class ImControls
    {
        public static ImRect AddRowRect(ImGui gui, ImSize size)
        {
            return size.Type switch
            {
                ImSizeType.Fixed => gui.Layout.AddRect(size.Width, size.Height),
                _ => gui.Layout.AddRect(gui.Layout.GetAvailableWidth(), gui.GetRowHeight())
            };
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeight(ImTheme.Active.Controls.TextSize) + ImTheme.Active.Controls.ExtraRowHeight;
        }

        public static float GetRowsHeightWithSpacing(this ImGui gui, int rows)
        {
            return Mathf.Max(0, gui.GetRowHeight() * rows + ImTheme.Active.Controls.ControlsSpacing * (rows - 1));
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
    
    [Serializable]
    public struct ImControlsStyle
    {
        public float ExtraRowHeight;
        public float TextSize;
        public float ControlsSpacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Indent;
    }
}