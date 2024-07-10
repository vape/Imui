using System;
using Imui.Controls.Styling;
using Imui.Core;

namespace Imui.Controls
{
    // TODO (artem-s): implement table layout helper
    public static class ImControls
    {
        public static ImControlsStyle Style = ImControlsStyle.Default;

        public static float TextSize => Style.TextSize;
        public static ImPadding Padding => Style.Padding;
        public static float InnerSpacing => Style.InnerSpacing;

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
            return gui.TextDrawer.GetLineHeight(Style.TextSize) + Style.Padding.Bottom + Style.Padding.Top;
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
            gui.Layout.AddSpace(Style.ControlsSpacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }

        public static void BeginIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(Style.Indent);
        }

        public static void EndIndent(this ImGui gui)
        {
            gui.Layout.AddIndent(-Style.Indent);
        }
    }
    
    public struct ImControlsStyle
    {
        public static readonly ImControlsStyle Default = new ImControlsStyle()
        {
            Padding = 2,
            TextSize = 26,
            ControlsSpacing = 4,
            InnerSpacing = 2,
            ScrollSpeedScale = 6,
            Indent = 20
        };

        public ImPadding Padding;
        public float TextSize;
        public float ControlsSpacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Indent;
    }
}