using Imui.Core;

namespace Imui.Controls
{
    public static class ImControlsLayout
    {
        public static ImControlsLayoutStyle Style = ImControlsLayoutStyle.Default;

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
    }

    public struct ImControlsLayoutStyle
    {
        public static readonly ImControlsLayoutStyle Default = new ImControlsLayoutStyle()
        {
            TextSize = 26,
            Spacing = 4
        };
        
        public float TextSize;
        public float Spacing;
    }
}