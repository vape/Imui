using System;
using Imui.Core;

namespace Imui.Controls
{
    // TODO (artem-s): implement table layout helper
    public static class ImControls
    {
        public static ImControlsStyle Style = ImControlsStyle.Default;
        
        [Obsolete]
        public static float GetTextSize(this ImGui gui)
        {
            return Style.TextSize;
        }
        
        public static float GetRowHeight(this ImGui gui)
        {
            return gui.TextDrawer.GetLineHeight(Style.TextSize);
        }
        
        public static void AddControlSpacing(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            if (frame.Size.x != 0 || frame.Size.y != 0)
            {
                gui.AddSpacing();
            }
        }
        
        public static void AddSpacing(this ImGui gui)
        {
            gui.Layout.AddSpace(Style.Spacing);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }

        public static void BeginIdent(this ImGui gui)
        {
            gui.Layout.AddIdent(Style.Ident);
        }

        public static void EndIdent(this ImGui gui)
        {
            gui.Layout.AddIdent(-Style.Ident);
        }
    }
    
    public struct ImControlsStyle
    {
        public static readonly ImControlsStyle Default = new ImControlsStyle()
        {
            TextSize = 26,
            Spacing = 4,
            InnerSpacing = 2,
            ScrollSpeedScale = 6,
            Ident = 20
        };
        
        public float TextSize;
        public float Spacing;
        public float InnerSpacing;
        public float ScrollSpeedScale;
        public float Ident;
    }
}