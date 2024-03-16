using Imui.Core;
using UnityEngine;

namespace Imui.Controls
{
    public static class ImPopup
    {
        public const int ORDER_CONTENT = 1024 * 1024 * 16;
        public const int ORDER_CLOSE_BUTTON = ORDER_CONTENT - 16;
        
        public static void BeginPopup(this ImGui gui, Vector2 size = default)
        {
            gui.Canvas.PushOrder(ORDER_CONTENT);
            gui.Layout.Push(size, ImAxis.Vertical);
            gui.Layout.MakeRoot();
        }

        public static void EndPopup(this ImGui gui, out bool close)
        {
            gui.Layout.Pop();
            gui.Canvas.PopOrder();

            close = CloseButton(gui);
        }

        private static bool CloseButton(ImGui gui)
        {
            var result = false;
            
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushOrder(ORDER_CLOSE_BUTTON);
            if (gui.InvisibleButton(gui.Canvas.ScreenRect))
            {
                result = true;
            }
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();

            return result;
        }
    }
}