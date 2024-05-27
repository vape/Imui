using Imui.Core;

namespace Imui.Controls
{
    public static class ImControlsLayout
    {
        public const float DEFAULT_CONTROL_SIZE = 32;
        public const float DEFAULT_SPACING = 4;

        public static void TryAddControlSpacing(this ImGui gui)
        {
            ref readonly var frame = ref gui.Layout.GetFrame();
            if (frame.Size.x != 0 || frame.Size.y != 0)
            {
                gui.AddSpacing();
            }
        }
        
        public static void AddSpacing(this ImGui gui)
        {
            gui.Layout.AddSpace(DEFAULT_SPACING);
        }
        
        public static void AddSpacing(this ImGui gui, float space)
        {
            gui.Layout.AddSpace(space);
        }
    }
}