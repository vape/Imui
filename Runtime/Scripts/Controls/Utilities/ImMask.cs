using Imui.Core;

namespace Imui.Controls.Utilities
{
    public static class ImMask
    {
        public static void BeginMaskedLayout(this ImGui gui)
        {
            var rect = gui.Layout.GetBoundsRect();
            gui.Canvas.PushClipRect(rect);
        }

        public static void EndMaskedLayout(this ImGui gui)
        {
            gui.Canvas.PopClipRect();
        }
    }
}