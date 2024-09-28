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
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushNoRectMask();
            gui.Canvas.PushOrder(ORDER_CONTENT);
        }

        public static void EndPopup(this ImGui gui, out bool close)
        {
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();
            gui.Canvas.PopRectMask();

            close = CloseButton(gui);
        }

        public static bool CloseButton(ImGui gui)
        {
            gui.Canvas.PushNoClipRect();
            gui.Canvas.PushOrder(ORDER_CLOSE_BUTTON);
            gui.RegisterRaycastTarget(gui.Canvas.ScreenRect);

            var id = gui.GetNextControlId();
            
            ref readonly var mouseEvent = ref gui.Input.MouseEvent;
            if (mouseEvent.Type == ImMouseEventType.Scroll || mouseEvent.Type == ImMouseEventType.Drag)
            {
                gui.Input.UseMouseEvent();
            }
            
            var clicked = gui.InvisibleButton(id, gui.Canvas.ScreenRect, ImButtonFlag.ActOnPress);
            
            gui.Canvas.PopOrder();
            gui.Canvas.PopClipRect();

            return clicked;
        }
    }
}