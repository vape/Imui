using Imui.Core;
using Imui.IO.Events;

namespace Imui.Controls
{
    public static class ImPopup
    {
        public const int ORDER_CONTENT = 1024 * 1024 * 16;
        public const int ORDER_CLOSE_BUTTON = ORDER_CONTENT - 16;
        
        public static void BeginPopup(this ImGui gui)
        {
            gui.Canvas.PushOrder(ORDER_CONTENT);
        }

        public static void EndPopup(this ImGui gui, out bool close)
        {
            gui.Canvas.PopOrder();

            close = CloseButton(gui);
        }

        public static bool CloseButton(ImGui gui)
        {
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushOrder(ORDER_CLOSE_BUTTON);

            var id = gui.GetNextControlId();
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            if (mouseEvent.Type == ImMouseEventType.Scroll)
            {
                gui.Input.UseMouseEvent();
            }
            
            var clicked = gui.InvisibleButton(id, gui.Canvas.ScreenRect);
            
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();

            return clicked;
        }
    }
}